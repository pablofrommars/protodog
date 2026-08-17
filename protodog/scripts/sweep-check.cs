#!/usr/bin/env dotnet
// sweep-check.cs — deterministic sweep-safety survey for the plan/ tree.
// Usage: dotnet sweep-check.cs [<repo-root>] [--base <ref>]   (defaults: cwd, main)
// Classifies every top-level plan/ entry and, for plan directories, proves the mechanically
// decidable retirement conditions: every plan file terminal, every obligation lifted
// (disposition arrows), and content committed to the base ref (identical to it). Citation liveness is
// judgment and is not checked here; the plan-sweep skill owns it.
// Exit 0: survey completed. Exit 2: usage or environment failure.
using System.Text.RegularExpressions;

var repoRoot = Environment.CurrentDirectory;
var baseRef = "main";
for (var i = 0; i < args.Length; i++)
{
	if (args[i] == "--base" && i + 1 < args.Length)
	{
		baseRef = args[++i];
	}
	else if (!args[i].StartsWith('-'))
	{
		repoRoot = Path.GetFullPath(args[i]);
	}
	else
	{
		Console.Error.WriteLine("usage: sweep-check.cs [<repo-root>] [--base <ref>]");
		Environment.Exit(2);
		return;
	}
}

var planRoot = Path.Combine(repoRoot, "plan");
if (!Directory.Exists(planRoot))
{
	Console.Error.WriteLine($"no plan/ directory under {repoRoot}");
	Environment.Exit(2);
	return;
}

string[] terminal = ["completed", "superseded", "cancelled"];
var arrow = new Regex(@"→ (D-\d{2,}|ISSUE-\d{2,}|closed: )");
var baseAvailable = Git("rev-parse", "--verify", "--quiet", $"{baseRef}^{{commit}}") is not null;
if (!baseAvailable)
{
	Console.WriteLine($"note: ref \"{baseRef}\" not found — committed state reported as unknown");
}

var sweepable = 0;
foreach (var entry in Directory.EnumerateFileSystemEntries(planRoot).Order())
{
	var name = Path.GetFileName(entry);
	if (File.Exists(entry))
	{
		if (name == "deferred.md")
		{
			var text = File.ReadAllText(entry);
			Console.WriteLine($"deferred.md: register — {Regex.Count(text, "· parked")} parked, {Regex.Count(text, "· closed|· dropped")} closed");
		}
		else
		{
			Console.WriteLine($"{name}: foreign — not a plan directory or the register");
		}

		continue;
	}

	var plans = Directory.EnumerateFiles(entry, "*-plan.md", SearchOption.AllDirectories)
		.Where(f => Path.GetFileName(f) is "task-plan.md" or "program-plan.md" or "track-plan.md")
		.ToList();
	if (plans.Count == 0)
	{
		Console.WriteLine($"{name}/: foreign — no plan artifact inside");
		continue;
	}

	var live = new List<string>();
	var unlifted = 0;
	foreach (var plan in plans)
	{
		var lines = File.ReadAllLines(plan);
		var status = lines
			.Select(l => Regex.Match(l, @"^- Status: (.+)$"))
			.FirstOrDefault(m => m.Success)?.Groups[1].Value.Trim();
		if (status is null || !terminal.Contains(status))
		{
			live.Add($"{Path.GetRelativePath(entry, plan)} · {status ?? "no status"}");
			continue;
		}

		unlifted += Unlifted(lines);
	}

	if (live.Count > 0)
	{
		Console.WriteLine($"{name}/: live — {string.Join(", ", live)}");
		continue;
	}

	var committed = baseAvailable ? Committed($"plan/{name}") : "unknown";
	var problems = new List<string>();
	if (unlifted > 0)
	{
		problems.Add($"{unlifted} unlifted item(s); legacy pre-3.0 plans record their lift in the register — engineer override required");
	}

	if (committed != "committed")
	{
		problems.Add(committed == "unknown" ? "committed state unknown" : $"not committed ({committed})");
	}

	if (problems.Count == 0)
	{
		sweepable++;
		Console.WriteLine($"{name}/: sweepable — terminal, lifted, committed; pending citation review");
	}
	else
	{
		Console.WriteLine($"{name}/: blocked — {string.Join("; ", problems)}");
	}
}

Console.WriteLine($"survey complete: {sweepable} mechanically sweepable (citation review still required)");

int Unlifted(string[] lines)
{
	var count = 0;
	var inSection = false;
	var inFence = false;
	foreach (var line in lines)
	{
		if (line.TrimStart().StartsWith("```"))
		{
			inFence = !inFence;
			continue;
		}

		if (inFence)
		{
			continue;
		}

		if (line.StartsWith("## "))
		{
			inSection = line is "## Deferred issues and accepted gaps" or "## Program issue ledger";
			continue;
		}

		if (inSection && line.StartsWith("- ") && !arrow.IsMatch(line))
		{
			count++;
		}
	}

	return count;
}

string Committed(string path)
{
	if (Git("ls-tree", "-d", baseRef, "--", path) is not { Length: > 0 })
	{
		return "branch-only";
	}

	return GitExit("diff", "--quiet", baseRef, "--", path) == 0 ? "committed" : $"diverged from {baseRef}";
}

string? Git(params string[] arguments)
{
	var (code, stdout) = Run(arguments);
	return code == 0 ? stdout.Trim() : null;
}

int GitExit(params string[] arguments)
{
	return Run(arguments).Code;
}

(int Code, string Stdout) Run(string[] arguments)
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

	using var process = System.Diagnostics.Process.Start(info)!;
	var stdout = process.StandardOutput.ReadToEnd();
	process.StandardError.ReadToEnd();
	process.WaitForExit();
	return (process.ExitCode, stdout);
}
