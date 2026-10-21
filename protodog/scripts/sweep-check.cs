#!/usr/bin/env dotnet
// sweep-check.cs — deterministic sweep-safety survey for the plan/ tree, specs/, and inputs/.
// Usage: dotnet sweep-check.cs [<repo-root>] [--base <ref>]   (defaults: cwd, main)
// Classifies every top-level plan/ entry and, for plan directories, proves the mechanically
// decidable retirement conditions: every plan file terminal, every obligation lifted
// (disposition arrows), and content committed to the base ref (identical to it). Then classifies
// every file under specs/ and inputs/ by citation and history: live (cited by a live plan or a
// live spec), retiring (cited only by sweepable plan directories), retirable (cited by nothing
// now, consumed by a plan in the base ref's history), backlog (cited by nothing, never
// consumed), or blocked (follows a blocked plan, or not committed). Documentation is
// self-contained: a docs/ citation into inputs/ or specs/ is reported as a defect, never as a
// reason to keep. Citation liveness beyond this is judgment; the plan-sweep skill owns it.
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
var arrow = new Regex(@"→ (D-\d{2,}|closed: )");
var baseAvailable = Git("rev-parse", "--verify", "--quiet", $"{baseRef}^{{commit}}") is not null;
if (!baseAvailable)
{
	Console.WriteLine($"note: ref \"{baseRef}\" not found — committed state reported as unknown");
}

var sweepable = 0;
var planState = new Dictionary<string, string>();
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
		.Where(f => Path.GetFileName(f) == "task-plan.md")
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
		planState[name] = "live";
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
		planState[name] = "sweepable";
		Console.WriteLine($"{name}/: sweepable — terminal, lifted, committed; pending citation review");
	}
	else
	{
		planState[name] = "blocked";
		Console.WriteLine($"{name}/: blocked — {string.Join("; ", problems)}");
	}
}

// Specs first, then inputs: an input cited by a spec is as live as that spec.
var retirable = 0;
var backlog = 0;
var fileState = new Dictionary<string, string>();
var consumedCache = new Dictionary<string, string?>();
foreach (var folder in new[] { "specs", "inputs" })
{
	var root = Path.Combine(repoRoot, folder);
	if (!Directory.Exists(root))
	{
		continue;
	}

	foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Order())
	{
		var relative = Path.GetRelativePath(repoRoot, file).Replace('\\', '/');
		var (state, detail) = Classify(relative);
		fileState[relative] = state;
		if (state == "retirable")
		{
			retirable++;
		}
		else if (state == "backlog")
		{
			backlog++;
		}

		Console.WriteLine($"{relative}: {state} — {detail}");
	}
}

Console.WriteLine($"survey complete: {sweepable} mechanically sweepable, {retirable} spec/input file(s) retirable, {backlog} backlog (citation review still required)");

(string State, string Detail) Classify(string relative)
{
	var citedByLive = new List<string>();
	var citedBySweepable = new List<string>();
	var citedByBlocked = new List<string>();
	foreach (var (plan, state) in planState)
	{
		if (!Cites(Path.Combine(planRoot, plan), relative))
		{
			continue;
		}

		(state switch { "live" => citedByLive, "sweepable" => citedBySweepable, _ => citedByBlocked }).Add($"plan/{plan}");
	}

	if (relative.StartsWith("inputs/"))
	{
		foreach (var (spec, state) in fileState.Where(f => f.Key.StartsWith("specs/")))
		{
			if (!Cites(Path.Combine(repoRoot, spec), relative))
			{
				continue;
			}

			(state switch { "live" => citedByLive, "retiring" => citedBySweepable, _ => citedByBlocked }).Add(spec);
		}
	}

	var docsRoot = Path.Combine(repoRoot, "docs");
	var docsCitations = Directory.Exists(docsRoot)
		? Directory.EnumerateFiles(docsRoot, "*", SearchOption.AllDirectories).Where(d => Cites(d, relative)).Select(d => Path.GetRelativePath(repoRoot, d).Replace('\\', '/')).ToList()
		: [];
	var defect = docsCitations.Count > 0 ? $"; defect: cited by {string.Join(", ", docsCitations)} — documentation is self-contained, cite history instead" : "";

	if (citedByLive.Count > 0)
	{
		return ("live", $"cited by {string.Join(", ", citedByLive)}{defect}");
	}

	if (citedByBlocked.Count > 0)
	{
		return ("blocked", $"follows {string.Join(", ", citedByBlocked)}{defect}");
	}

	if (citedBySweepable.Count > 0)
	{
		return ("retiring", $"with {string.Join(", ", citedBySweepable)}{defect}");
	}

	if (!baseAvailable)
	{
		return ("blocked", $"cited by nothing; history unknown without the base ref{defect}");
	}

	var committed = Committed(relative);
	if (committed != "committed")
	{
		return ("blocked", $"cited by nothing; not committed ({committed}) — nothing archives it{defect}");
	}

	var consumed = Consumed(relative);
	return consumed is { Length: > 0 }
		? ("retirable", $"consumed in history ({consumed}); nothing cites it now{defect}")
		: ("backlog", $"never consumed by a plan{defect}");
}

// A citation names the file or any directory above it below specs/ or inputs/: a plan that
// grounds on `inputs/data-room/` consumed every file inside.
IEnumerable<string> CitationForms(string relative)
{
	yield return relative;
	var parent = Path.GetDirectoryName(relative)?.Replace('\\', '/');
	while (parent is { Length: > 0 } && parent.Contains('/'))
	{
		yield return parent;
		parent = Path.GetDirectoryName(parent)?.Replace('\\', '/');
	}
}

bool Cites(string path, string relative)
{
	if (Directory.Exists(path))
	{
		return Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories).Any(f => Cites(f, relative));
	}

	if (!File.Exists(path) || Path.GetRelativePath(repoRoot, path).Replace('\\', '/') == relative)
	{
		return false;
	}

	var text = File.ReadAllText(path);
	return CitationForms(relative).Any(form => text.Contains(form));
}

// History is searched once per citation form; a folder of samples asks for its folder once.
string? Consumed(string relative)
{
	foreach (var form in CitationForms(relative))
	{
		if (!consumedCache.TryGetValue(form, out var hit))
		{
			hit = Git("log", "--format=%h", "-1", "-S", form, baseRef, "--", "plan", "specs");
			consumedCache[form] = hit is { Length: > 0 } ? hit : null;
		}

		if (consumedCache[form] is { } commit)
		{
			return commit;
		}
	}

	return null;
}

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
			inSection = line == "## Deferred issues and accepted gaps";
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
	if (Git("ls-tree", baseRef, "--", path) is not { Length: > 0 })
	{
		return "branch-only";
	}

	// git diff cannot see untracked files; the working tree may carry nothing the base ref lacks.
	if (Git("status", "--porcelain", "--untracked-files=all", "--", path) is { Length: > 0 })
	{
		return "uncommitted working-tree content";
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
