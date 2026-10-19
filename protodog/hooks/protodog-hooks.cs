#!/usr/bin/env dotnet
// protodog-hooks.cs — Protocol enforcement hooks as one .NET file-based app.
// Subcommands: pre-bash | post-bash | pre-write | post-plan-write (PreToolUse Bash, PostToolUse
// Bash, PreToolUse Write|Edit, PostToolUse Write|Edit). Hook JSON arrives on stdin; exit 0
// allows, exit 2 denies with the reason on stderr (fed back to the agent); denials append to
// the log. pre-bash snapshots canonical state (plan/, specs/) and post-bash compares: an
// immutable artifact mutated through the shell is denied, a plan artifact changed through the
// shell is validated. Post-write paths also hold ids stable against HEAD once execution began.
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

if (args is not [var hook] || hook is not ("pre-bash" or "post-bash" or "pre-write" or "post-plan-write"))
{
	Console.Error.WriteLine("usage: protodog-hooks.cs <pre-bash|post-bash|pre-write|post-plan-write>");
	Environment.Exit(2);
	return;
}

var command = "";
var filePath = "";
var sessionId = "nosession";
try
{
	using var doc = JsonDocument.Parse(Console.In.ReadToEnd());
	if (doc.RootElement.TryGetProperty("session_id", out var sessionElement))
	{
		sessionId = sessionElement.GetString() ?? sessionId;
	}

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
	case "post-bash":
	{
		PostBash();
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
		(@"\bgit\b[^\n|;&]*\bpush\b", "publishing refs is engineer-owned (guarded commit-to-main/backup only)"),
		(@"\bgit\b[^\n|;&]*\bfetch\b", "synchronization with the remote is engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bpull\b", "synchronization with the remote is engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bremote\s+update\b", "synchronization with the remote is engineer-owned"),
		(@"\bgit\b[^\n|;&]*\brebase\b", "rebase (including onto main) is engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bfilter-branch\b", "history rewrites are engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bcommit\b[^\n|;&]*--amend", "history rewrites are engineer-owned"),
		(@"\bgit\b[^\n|;&]*\breset\b[^\n|;&]*--hard", "hard resets are engineer-owned recovery actions"),
		(@"\bgit\b[^\n|;&]*\breset\b[^\n|;&]*--soft", "moving the branch tip is a history rewrite, engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bbranch\b[^\n|;&]*\s-[dD]\b", "branch deletion is engineer-owned cleanup"),
		(@"\bgit\b[^\n|;&]*\b(checkout|switch)\s+(origin/)?main\b", "the main branch is not an agent workspace"),
		(@"\bgit\b[^\n|;&]*\bmerge\s+(origin/)?main\b", "never merge main into a profile branch; syncing for the commit to main is rebase, engineer-owned"),
		(@"\bgit\b[^\n|;&]*\bworktree\s+(remove|prune)\b", "worktree cleanup is engineer-owned"),
	];

	foreach (var (pattern, why) in denied)
	{
		if (Regex.IsMatch(command, pattern))
		{
			Deny(why);
		}
	}

	// Snapshot canonical state so post-bash can see what the command did to it.
	var snapshot = SnapshotPath();
	Directory.CreateDirectory(Path.GetDirectoryName(snapshot)!);
	foreach (var stale in Directory.EnumerateFiles(Path.GetDirectoryName(snapshot)!).Where(f => File.GetLastWriteTimeUtc(f) < DateTime.UtcNow.AddDays(-1)))
	{
		File.Delete(stale);
	}

	File.WriteAllLines(snapshot, Canonical().Select(kv => $"{kv.Value} {kv.Key}"));
}

// Shell commands reach canonical state outside the Write/Edit fences; this is the fence for that
// path. Deletions are not flagged — the engineer-triggered retirement sweep deletes through here.
void PostBash()
{
	var snapshot = SnapshotPath();
	if (!File.Exists(snapshot))
	{
		return;
	}

	var before = File.ReadAllLines(snapshot)
		.Select(line => line.Split(' ', 2))
		.Where(parts => parts.Length == 2)
		.ToDictionary(parts => parts[1], parts => parts[0]);
	File.Delete(snapshot);
	var after = Canonical();
	var problems = new List<string>();
	foreach (var (path, hash) in before)
	{
		if (!after.TryGetValue(path, out var now) || now == hash)
		{
			continue;
		}

		if (Regex.IsMatch(path, @"(^|/)audits/audit-\d{2,}\.md$"))
		{
			problems.Add($"audit report is immutable once written: {path}");
		}
		else if (path.StartsWith("specs/") && BoundBy("@" + path) is { } plan)
		{
			problems.Add($"spec {path} is bound by {plan} and immutable — create a superseding spec instead");
		}
	}

	var changedPlans = after
		.Where(kv => IsPlanArtifact(kv.Key) && (!before.TryGetValue(kv.Key, out var was) || was != kv.Value))
		.Select(kv => Path.Combine(repoRoot, kv.Key))
		.ToList();
	if (changedPlans.Count > 0)
	{
		if (Validate(changedPlans) is { } failure)
		{
			problems.Add(failure.TrimEnd());
		}

		problems.AddRange(changedPlans.SelectMany(StableIdProblems));
	}

	if (problems.Count > 0)
	{
		Deny("shell-side mutation of canonical state:\n" + string.Join("\n", problems));
	}
}

string SnapshotPath()
{
	var key = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(repoRoot + "\n" + command)))[..16];
	return Path.Combine(Path.GetTempPath(), "protodog-snapshots", $"{sessionId}-{key}.txt");
}

Dictionary<string, string> Canonical()
{
	var result = new Dictionary<string, string>();
	foreach (var top in new[] { "plan", "specs" })
	{
		var directory = Path.Combine(repoRoot, top);
		if (!Directory.Exists(directory))
		{
			continue;
		}

		foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
		{
			var relative = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');
			result[relative] = Convert.ToHexStringLower(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(file)));
		}
	}

	return result;
}

bool IsPlanArtifact(string relative)
{
	var baseName = Path.GetFileName(relative);
	return relative == "plan/deferred.md"
		|| baseName is "task-plan.md" or "program-plan.md" or "track-plan.md"
		|| Regex.IsMatch(baseName, @"^DISPATCH-\d+.*\.md$");
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

	var reference = "@" + Path.GetRelativePath(repoRoot, abs).Replace('\\', '/');
	if (BoundBy(reference) is { } plan)
	{
		Deny($"spec is bound by {plan} and immutable — create a superseding spec instead");
	}
}

// A spec is bound the moment any plan past "pending planning" references it; bound specs are
// immutable and changed intent must supersede rather than edit. Returns the binding plan.
string? BoundBy(string reference)
{
	var planRoot = Path.Combine(repoRoot, "plan");
	if (!Directory.Exists(planRoot))
	{
		return null;
	}

	foreach (var plan in Directory.EnumerateFiles(planRoot, "*-plan.md", SearchOption.AllDirectories))
	{
		var text = File.ReadAllText(plan);
		var match = Regex.Match(text, @"^- Status: (.+)$", RegexOptions.Multiline);
		var status = match.Success ? match.Groups[1].Value.Trim() : "";
		if (status.Length > 0 && status != "pending planning" && text.Contains(reference))
		{
			return $"{Path.GetRelativePath(repoRoot, plan)} (status: {status})";
		}
	}

	return null;
}

void PostPlanWrite()
{
	var baseName = Path.GetFileName(filePath);
	var isRegister = baseName == "deferred.md" && Regex.IsMatch(Path.GetFullPath(filePath).Replace('\\', '/'), @"(^|/)plan/deferred\.md$");
	var isPlan = isRegister || baseName is "task-plan.md" or "program-plan.md" or "track-plan.md" || Regex.IsMatch(baseName, @"^DISPATCH-\d+.*\.md$");
	if (!isPlan)
	{
		return;
	}

	if (Validate([filePath]) is { } failure)
	{
		Deny($"plan artifact failed validation:\n{failure}");
	}

	var drift = StableIdProblems(filePath).ToList();
	if (drift.Count > 0)
	{
		Deny(string.Join("\n", drift));
	}
}

// Runs the plan validator over the given paths; returns its stderr on failure, null on success.
string? Validate(IReadOnlyList<string> paths)
{
	var validator = Path.Combine(pluginRoot, "validators", "validate-plan.cs");
	var startInfo = new System.Diagnostics.ProcessStartInfo("dotnet")
	{
		RedirectStandardError = true,
		RedirectStandardOutput = true,
	};
	startInfo.ArgumentList.Add(validator);
	foreach (var path in paths)
	{
		startInfo.ArgumentList.Add(path);
	}

	using var process = System.Diagnostics.Process.Start(startInfo)!;
	var stderr = process.StandardError.ReadToEnd();
	process.WaitForExit();
	return process.ExitCode == 0 ? null : stderr;
}

// Durable ids are stable after execution begins: a write that drops an id present in the last
// committed version renumbered or reused it. The validator sees one artifact; this is the only
// place that can see two. The register's D-ids are stable from the first row.
IEnumerable<string> StableIdProblems(string path)
{
	var relative = Path.GetRelativePath(repoRoot, Path.GetFullPath(path)).Replace('\\', '/');
	if (!IsPlanArtifact(relative) || Regex.IsMatch(Path.GetFileName(relative), @"^DISPATCH-") || !File.Exists(path))
	{
		yield break;
	}

	var committed = Git("show", $"HEAD:{relative}");
	if (committed is null)
	{
		yield break;
	}

	if (relative != "plan/deferred.md")
	{
		var status = Regex.Match(committed, @"^- Status: (.+)$", RegexOptions.Multiline);
		if (!status.Success || status.Groups[1].Value.Trim() is "pending planning" or "ready")
		{
			yield break;
		}
	}

	var idPattern = new Regex(@"^(?:\| (?:⬜|🟡|✅|🔴|⬛) (?:~~)?|- )([A-Z]+-\d{2,})\b", RegexOptions.Multiline);
	var before = idPattern.Matches(committed).Select(m => m.Groups[1].Value).ToHashSet();
	var now = idPattern.Matches(File.ReadAllText(path)).Select(m => m.Groups[1].Value).ToHashSet();
	foreach (var id in before.Except(now).OrderBy(id => id))
	{
		yield return $"{relative}: {id} is present at HEAD and gone from this write — ids are stable after execution begins; supersede and link instead";
	}
}

string? Git(params string[] arguments)
{
	var info = new System.Diagnostics.ProcessStartInfo("git")
	{
		WorkingDirectory = repoRoot,
		RedirectStandardOutput = true,
		RedirectStandardError = true,
	};
	foreach (var argument in arguments)
	{
		info.ArgumentList.Add(argument);
	}

	try
	{
		using var process = System.Diagnostics.Process.Start(info)!;
		var stdout = process.StandardOutput.ReadToEnd();
		process.StandardError.ReadToEnd();
		process.WaitForExit();
		return process.ExitCode == 0 ? stdout : null;
	}
	catch (System.ComponentModel.Win32Exception)
	{
		return null;
	}
}
