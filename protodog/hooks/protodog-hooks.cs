#!/usr/bin/env dotnet
// protodog-hooks.cs — Protocol enforcement hooks as one .NET file-based app.
// Subcommands: pre-bash | pre-write | post-plan-write (PreToolUse Bash, PreToolUse Write|Edit,
// PostToolUse Write|Edit). Hook JSON arrives on stdin; exit 0 allows, exit 2 denies with the
// reason on stderr (fed back to the agent); denials append to the log.
// Resolution: repo state comes from CLAUDE_PROJECT_DIR (else cwd); plugin files from
// CLAUDE_PLUGIN_ROOT (else repo-local protodog/); the denial log honors
// PROTOCOL_DENIAL_LOG, then CLAUDE_PLUGIN_DATA, then the repo-local logs directory.
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

var repoRoot = Environment.GetEnvironmentVariable("CLAUDE_PROJECT_DIR") ?? Environment.CurrentDirectory;
var pluginRoot = Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_ROOT") ?? Path.Combine(repoRoot, "protodog");
var pluginData = Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_DATA");
var denialLog = Environment.GetEnvironmentVariable("PROTOCOL_DENIAL_LOG")
	?? (pluginData is not null
		? Path.Combine(pluginData, "protocol-denials.log")
		: Path.Combine(repoRoot, "protodog", "logs", "hook-denials.log"));

if (args is not [var hook] || hook is not ("pre-bash" or "pre-write" or "post-plan-write"))
{
	Console.Error.WriteLine("usage: protodog-hooks.cs <pre-bash|pre-write|post-plan-write>");
	Environment.Exit(2);
	return;
}

var command = "";
var filePath = "";
try
{
	using var doc = JsonDocument.Parse(Console.In.ReadToEnd());
	if (doc.RootElement.TryGetProperty("tool_input", out var toolInput))
	{
		if (toolInput.TryGetProperty("command", out var commandElement))
		{
			command = commandElement.GetString() ?? "";
		}

		if (toolInput.TryGetProperty("file_path", out var fileElement))
		{
			filePath = fileElement.GetString() ?? "";
		}
	}
}
catch (JsonException)
{
	// Unparseable input allows: enforcement must not break unrelated tools.
}

switch (hook)
{
	case "pre-bash":
	{
		PreBash();
		break;
	}
	case "pre-write":
	{
		PreWrite();
		break;
	}
	case "post-plan-write":
	{
		PostPlanWrite();
		break;
	}
}

Environment.Exit(0);

void Deny(string detail)
{
	try
	{
		Directory.CreateDirectory(Path.GetDirectoryName(denialLog)!);
		var entry = new JsonObject
		{
			["ts"] = DateTime.UtcNow.ToString("o"),
			["hook"] = hook,
			["detail"] = detail,
			["command"] = command,
			["file"] = filePath,
		};
		File.AppendAllText(denialLog, entry.ToJsonString() + "\n");
	}
	catch (IOException)
	{
		// Logging must never mask the denial.
	}

	Console.Error.WriteLine($"protocol hook ({hook}): {detail}");
	Environment.Exit(2);
}

void PreBash()
{
	(string Pattern, string Why)[] denied =
	[
		(@"\bgit\b[^\n|;&]*\bpush\b", "publishing refs is engineer-owned (guarded landing/backup only)"),
		(@"\bgit\b[^\n|;&]*\bfetch\b", "synchronization with the remote is engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bpull\b", "synchronization with the remote is engineer-owned"),
		(@"\bgit\b[^\n|;&]*\brebase\b", "rebase (including onto main) is engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bfilter-branch\b", "history rewrites are engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bcommit\b[^\n|;&]*--amend", "history rewrites are engineer-owned"),
		(@"\bgit\b[^\n|;&]*\breset\b[^\n|;&]*--hard", "hard resets are engineer-owned recovery actions"),
		(@"\bgit\b[^\n|;&]*\bbranch\b[^\n|;&]*\s-[dD]\b", "branch deletion is engineer-owned cleanup"),
		(@"\bgit\b[^\n|;&]*\b(checkout|switch)\s+(origin/)?main\b", "the main branch is not an agent workspace"),
		(@"\bgit\b[^\n|;&]*\bmerge\s+(origin/)?main\b", "never merge main into a landing branch; landing sync is rebase, engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bworktree\s+(remove|prune)\b", "worktree cleanup is engineer-owned"),
	];

	foreach (var (pattern, why) in denied)
	{
		if (Regex.IsMatch(command, pattern))
		{
			Deny(why);
		}
	}
}

void PreWrite()
{
	if (filePath.Length == 0)
	{
		return;
	}

	var abs = Path.GetFullPath(filePath);
	if (Regex.IsMatch(abs.Replace('\\', '/'), @"(^|/)audits/audit-\d{2,}\.md$") && File.Exists(abs))
	{
		Deny($"audit report is immutable once written: {Path.GetRelativePath(repoRoot, abs)}");
	}

	var specsDir = Path.Combine(repoRoot, "specs") + Path.DirectorySeparatorChar;
	if (!abs.StartsWith(specsDir) || !File.Exists(abs))
	{
		return;
	}

	// A spec is bound the moment any plan past "pending planning" references it; bound specs
	// are immutable and changed intent must supersede rather than edit.
	var reference = "@" + Path.GetRelativePath(repoRoot, abs).Replace('\\', '/');
	var planRoot = Path.Combine(repoRoot, "plan");
	if (!Directory.Exists(planRoot))
	{
		return;
	}

	foreach (var plan in Directory.EnumerateFiles(planRoot, "*-plan.md", SearchOption.AllDirectories))
	{
		var text = File.ReadAllText(plan);
		var match = Regex.Match(text, @"^- Status: (.+)$", RegexOptions.Multiline);
		var status = match.Success ? match.Groups[1].Value.Trim() : "";
		if (status.Length > 0 && status != "pending planning" && text.Contains(reference))
		{
			Deny($"spec is bound by {Path.GetRelativePath(repoRoot, plan)} (status: {status}) and immutable — create a superseding spec instead");
		}
	}
}

void PostPlanWrite()
{
	var baseName = Path.GetFileName(filePath);
	var isPlan = baseName is "task-plan.md" or "program-plan.md" or "track-plan.md" || Regex.IsMatch(baseName, @"^DISPATCH-\d+.*\.md$");
	if (!isPlan)
	{
		return;
	}

	var validator = Path.Combine(pluginRoot, "validators", "validate-plan.cs");
	var startInfo = new System.Diagnostics.ProcessStartInfo("dotnet")
	{
		RedirectStandardError = true,
		RedirectStandardOutput = true,
	};
	startInfo.ArgumentList.Add(validator);
	startInfo.ArgumentList.Add(filePath);
	using var process = System.Diagnostics.Process.Start(startInfo)!;
	var stderr = process.StandardError.ReadToEnd();
	process.WaitForExit();
	if (process.ExitCode != 0)
	{
		Deny($"plan artifact failed validation:\n{stderr}");
	}
}
