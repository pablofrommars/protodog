#!/usr/bin/env dotnet
// worktree-survey.cs — report-only survey of a repository's worktrees for retirement candidates.
// Usage: dotnet worktree-survey.cs [<repo-root>] [--base <ref>]   (defaults: cwd, main)
// For every worktree except the main one it proves the mechanically decidable conditions: the
// worktree's HEAD is an ancestor of the base ref, or every commit it adds is patch-equivalent to
// one already in the base ref (a squash landing); and the working tree is clean. It lists the
// judgment items per entry — dirty paths, a detached HEAD, a branch with no live upstream,
// ignored-but-present files, a directory or branch name matching a plan directory at the base
// ref — prints the exact removal commands for each candidate, and runs none of them: worktree
// cleanup and branch deletion are engineer-owned. Stashes are repository-global and are counted
// once. Exit 0: survey completed. Exit 2: usage or environment failure.
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
		Console.Error.WriteLine("usage: worktree-survey.cs [<repo-root>] [--base <ref>]");
		Environment.Exit(2);
		return;
	}
}

if (Git(repoRoot, "rev-parse", "--show-toplevel") is null)
{
	Console.Error.WriteLine($"not a git repository: {repoRoot}");
	Environment.Exit(2);
	return;
}

var baseAvailable = Git(repoRoot, "rev-parse", "--verify", "--quiet", $"{baseRef}^{{commit}}") is not null;
if (!baseAvailable)
{
	Console.WriteLine($"note: ref \"{baseRef}\" not found — merged state reported as unknown");
}

var planIds = baseAvailable
	? (Git(repoRoot, "ls-tree", "-d", "--name-only", baseRef, "plan/") ?? "")
		.Split('\n', StringSplitOptions.RemoveEmptyEntries)
		.Select(p => p["plan/".Length..])
		.ToHashSet()
	: [];

var porcelain = Git(repoRoot, "worktree", "list", "--porcelain");
if (porcelain is null)
{
	Console.Error.WriteLine("git worktree list failed");
	Environment.Exit(2);
	return;
}

var entries = new List<(string Path, string Head, string? Branch)>();
string? path = null, head = null, branch = null;
foreach (var line in porcelain.Split('\n').Append(""))
{
	if (line.StartsWith("worktree "))
	{
		path = line["worktree ".Length..];
	}
	else if (line.StartsWith("HEAD "))
	{
		head = line["HEAD ".Length..];
	}
	else if (line.StartsWith("branch refs/heads/"))
	{
		branch = line["branch refs/heads/".Length..];
	}
	else if (line.Length == 0 && path is not null)
	{
		entries.Add((path, head ?? "", branch));
		path = head = branch = null;
	}
}

var candidates = 0;
foreach (var (worktreePath, worktreeHead, worktreeBranch) in entries.Skip(1))
{
	var name = Path.GetFileName(worktreePath.TrimEnd(Path.DirectorySeparatorChar));
	var blockers = new List<string>();
	var notes = new List<string>();

	// Proof 1: merged — an ancestor of the base ref, or patch-equivalent to it.
	string merged;
	if (!baseAvailable)
	{
		merged = "unknown";
		blockers.Add("merged state unknown");
	}
	else if (GitExit(repoRoot, "merge-base", "--is-ancestor", worktreeHead, baseRef) == 0)
	{
		merged = "ancestor";
	}
	else
	{
		var cherry = (Git(repoRoot, "cherry", baseRef, worktreeHead) ?? "")
			.Split('\n', StringSplitOptions.RemoveEmptyEntries);
		var ahead = cherry.Count(c => c.StartsWith('+'));
		if (cherry.Length > 0 && ahead == 0)
		{
			merged = "patch-equivalent";
		}
		else if (SquashLanded(worktreeHead))
		{
			merged = "squash-landed";
		}
		else
		{
			merged = "no";
			blockers.Add($"ahead {ahead} commit(s)");
		}
	}

	// Proof 2: clean — nothing modified, deleted, or untracked.
	var status = (Git(worktreePath, "status", "--porcelain", "--untracked-files=all") ?? "")
		.Split('\n', StringSplitOptions.RemoveEmptyEntries);
	if (status.Length > 0)
	{
		blockers.Add($"dirty {status.Length} path(s): {string.Join(", ", status.Take(3).Select(s => s.Trim()))}{(status.Length > 3 ? ", …" : "")}");
	}

	// Judgment items — reported, never decided here.
	if (worktreeBranch is null)
	{
		notes.Add("detached HEAD");
	}
	else
	{
		var upstream = Git(repoRoot, "for-each-ref", "--format=%(upstream:short)", $"refs/heads/{worktreeBranch}") ?? "";
		if (upstream.Length == 0)
		{
			notes.Add("branch never published");
		}
		else if (Git(repoRoot, "rev-parse", "--verify", "--quiet", upstream) is null)
		{
			notes.Add($"upstream {upstream} missing");
		}
	}

	var ignored = (Git(worktreePath, "status", "--porcelain", "--ignored", "--untracked-files=all") ?? "")
		.Split('\n', StringSplitOptions.RemoveEmptyEntries)
		.Count(s => s.StartsWith("!!"));
	if (ignored > 0)
	{
		notes.Add($"{ignored} ignored file(s) present");
	}

	foreach (var planId in planIds)
	{
		if (name == planId || name.StartsWith($"{planId}--") || worktreeBranch == planId)
		{
			notes.Add($"plan/{planId} still present at {baseRef}");
		}
	}

	var noteText = notes.Count > 0 ? $"; note: {string.Join("; ", notes)}" : "";
	if (blockers.Count == 0)
	{
		candidates++;
		if (worktreeBranch is not null && merged != "ancestor")
		{
			notes.Add("branch -d would refuse a squash landing; -D is printed");
			noteText = $"; note: {string.Join("; ", notes)}";
		}

		var branchCommand = worktreeBranch is null
			? ""
			: merged == "ancestor" ? $"; git branch -d {Quote(worktreeBranch)}" : $"; git branch -D {Quote(worktreeBranch)}";
		Console.WriteLine($"{name}: candidate — merged ({merged}), clean{noteText}; remove: git worktree remove {Quote(worktreePath)}{branchCommand}");
	}
	else
	{
		Console.WriteLine($"{name}: blocked — {string.Join("; ", blockers)}{noteText}");
	}
}

var stashes = (Git(repoRoot, "stash", "list") ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries).Length;
Console.WriteLine($"stashes: {stashes} (repository-wide, not attributed to a worktree)");
Console.WriteLine("ignored files are local configuration or build output: removal loses them");
Console.WriteLine($"survey complete: {candidates} candidate(s) of {Math.Max(entries.Count - 1, 0)} worktree(s); nothing was removed");

// A branch of several commits landed as one squash commit: no single commit is patch-equivalent,
// but the branch's cumulative diff since the merge base is the patch of one commit the base ref
// gained since then. Only base commits touching the branch's paths are compared.
bool SquashLanded(string worktreeHead)
{
	var mergeBase = Git(repoRoot, "merge-base", baseRef, worktreeHead);
	if (mergeBase is null)
	{
		return false;
	}

	var cumulative = PatchId(Run(repoRoot, ["diff", mergeBase, worktreeHead]).Stdout);
	if (cumulative is null)
	{
		return false;
	}

	var paths = (Git(repoRoot, "diff", "--name-only", mergeBase, worktreeHead) ?? "")
		.Split('\n', StringSplitOptions.RemoveEmptyEntries);
	var candidates = (Git(repoRoot, ["rev-list", $"{mergeBase}..{baseRef}", "--", .. paths]) ?? "")
		.Split('\n', StringSplitOptions.RemoveEmptyEntries);
	return candidates.Any(commit => PatchId(Run(repoRoot, ["diff-tree", "-p", "--root", commit]).Stdout) == cumulative);
}

string? PatchId(string patch)
{
	if (patch.Trim().Length == 0)
	{
		return null;
	}

	var info = new System.Diagnostics.ProcessStartInfo("git")
	{
		WorkingDirectory = repoRoot,
		RedirectStandardInput = true,
		RedirectStandardOutput = true,
		RedirectStandardError = true,
	};
	info.ArgumentList.Add("patch-id");
	info.ArgumentList.Add("--stable");
	using var process = System.Diagnostics.Process.Start(info)!;
	process.StandardInput.Write(patch);
	process.StandardInput.Close();
	var stdout = process.StandardOutput.ReadToEnd();
	process.StandardError.ReadToEnd();
	process.WaitForExit();
	var id = stdout.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
	return process.ExitCode == 0 && id is { Length: > 0 } ? id : null;
}

// Single-quoted for a POSIX shell: the only character needing care inside is the quote itself.
string Quote(string value)
{
	return "'" + value.Replace("'", "'\\''") + "'";
}

string? Git(string directory, params string[] arguments)
{
	var (code, stdout) = Run(directory, arguments);
	return code == 0 ? stdout.Trim() : null;
}

int GitExit(string directory, params string[] arguments)
{
	return Run(directory, arguments).Code;
}

(int Code, string Stdout) Run(string directory, string[] arguments)
{
	var info = new System.Diagnostics.ProcessStartInfo("git")
	{
		WorkingDirectory = directory,
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
