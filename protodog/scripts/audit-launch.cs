#!/usr/bin/env dotnet
// audit-launch.cs — guarded launcher for the external independent audit.
// Usage: dotnet audit-launch.cs [--dry-run] <path/to/audit-NN-dispatch.md>
// The dispatch path is the ONLY steering input; no free text reaches the auditor.
// Gates, in order: argument shape → dispatch exists and is named audit-NN-dispatch.md →
// report does not already exist (a re-launch is a new numbered cycle) → every manifest
// hash row in the dispatch re-verifies against the target repository → auditor CLI
// launches (skipped in --dry-run). Then: codex exec, read-only sandbox, pinned model,
// final message captured to audit-NN.md, provenance stamp prepended, report made
// read-only, launch logged.
// Resolution: repository root from CLAUDE_PROJECT_DIR, else cwd — invoke from the audited
// repository's root. Launch log honors PROTOCOL_AUDIT_LOG, then CLAUDE_PLUGIN_DATA, then
// the repo-local logs directory. Exit: 0 ok, 2 usage, 3 gate refusal.
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

var configuration = new Configuration();

var dryRun = args.Length > 0 && args[0] == "--dry-run";
var rest = dryRun ? args[1..] : args;
if (rest is not [var dispatchArg])
{
	Console.Error.WriteLine("usage: audit-launch.cs [--dry-run] <audit-NN-dispatch.md>");
	Environment.Exit(2);
	return;
}

var root = Environment.GetEnvironmentVariable("CLAUDE_PROJECT_DIR") ?? Environment.CurrentDirectory;
var pluginData = Environment.GetEnvironmentVariable("CLAUDE_PLUGIN_DATA");
var log = Environment.GetEnvironmentVariable("PROTOCOL_AUDIT_LOG")
	?? Path.Combine(pluginData ?? Path.Combine(root, "protodog", "logs"), "audit-launch.log");
Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(log))!);

var dispatch = Path.GetFullPath(dispatchArg);
var baseName = Path.GetFileName(dispatch);

void Log(string line)
{
	File.AppendAllText(log, $"{DateTime.UtcNow:yyyy-MM-dd'T'HH:mm:ss'Z'} {line}\n");
}

void Refuse(string reason)
{
	Console.Error.WriteLine($"REFUSED: {reason}");
	Log($"REFUSED {baseName}: {reason}");
	Environment.Exit(3);
}

if (!File.Exists(dispatch))
{
	Refuse($"dispatch not found: {dispatchArg}");
}

if (!Regex.IsMatch(baseName, @"^audit-\d{2}-dispatch\.md$"))
{
	Refuse("argument must be an audit-NN-dispatch.md file");
}

var number = baseName["audit-".Length..^"-dispatch.md".Length];
var output = Path.Combine(Path.GetDirectoryName(dispatch)!, $"audit-{number}.md");
if (File.Exists(output))
{
	Refuse($"audit-{number}.md already exists — a re-launch requires a new numbered cycle");
}

var manifestRow = new Regex("`([0-9a-f]{64})`[^`]*`([^`]+)`");
var verifiedRows = 0;
foreach (var line in File.ReadLines(dispatch))
{
	var match = manifestRow.Match(line);
	if (!match.Success)
	{
		continue;
	}

	var expected = match.Groups[1].Value;
	var file = match.Groups[2].Value;
	var target = Path.Combine(root, file);
	if (!File.Exists(target))
	{
		Refuse($"manifest file missing: {file}");
	}

	var actual = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(target)));
	if (actual != expected)
	{
		Refuse($"manifest hash mismatch: {file}");
	}

	verifiedRows++;
}

if (verifiedRows == 0)
{
	Refuse("dispatch contains no verifiable manifest rows");
}

var dispatchSha = Convert.ToHexStringLower(SHA256.HashData(File.ReadAllBytes(dispatch)));
var prompt = $"Execute the audit dispatch at {dispatch} exactly as written. Your final message must be the complete audit report Markdown required by the dispatch's report contract, and nothing else.";
List<string> arguments =
[
	"exec",
	"--sandbox", configuration.Sandbox,
	"-m", configuration.Model,
	"-C", root,
	"--output-last-message", output,
	prompt,
];

if (dryRun)
{
	Console.WriteLine($"DRY-RUN OK: {verifiedRows} manifest rows verified; dispatch sha256 {dispatchSha}");
	Console.WriteLine($"DRY-RUN command: {configuration.ExecutablePath} {string.Join(' ', arguments.Select(Quote))}");
	Log($"DRY-RUN {baseName} sha256={dispatchSha} rows={verifiedRows}");
	return;
}

Log($"LAUNCH {baseName} sha256={dispatchSha} rows={verifiedRows} model={configuration.Model}");
var startInfo = new ProcessStartInfo(configuration.ExecutablePath);
foreach (var argument in arguments)
{
	startInfo.ArgumentList.Add(argument);
}

Process process;
try
{
	process = Process.Start(startInfo)!;
}
catch (System.ComponentModel.Win32Exception)
{
	Refuse($"auditor CLI not available: {configuration.ExecutablePath}");
	return;
}

using (process)
{
	if (!process.WaitForExit(configuration.TimeoutSeconds * 1000))
	{
		process.Kill(entireProcessTree: true);
		Refuse($"auditor timed out after {configuration.TimeoutSeconds}s");
	}

	if (!File.Exists(output) || new FileInfo(output).Length == 0)
	{
		Refuse("auditor produced no report");
	}

	var stamp = $"<!-- launched by audit-launch.cs: dispatch {baseName} sha256={dispatchSha} model={configuration.Model} sandbox={configuration.Sandbox} -->";
	File.WriteAllText(output, $"{stamp}\n{File.ReadAllText(output)}");
	if (!OperatingSystem.IsWindows())
	{
		File.SetUnixFileMode(output, UnixFileMode.UserRead | UnixFileMode.GroupRead | UnixFileMode.OtherRead);
	}

	Log($"DONE {baseName} -> {Path.GetFileName(output)} exit={process.ExitCode}");
	Console.WriteLine($"audit report written: {output}");
}

static string Quote(string value)
{
	return value.Contains(' ') ? $"\"{value}\"" : value;
}

internal sealed record Configuration
{
	// Pinned auditor configuration — adapter-owned; change only through a reviewed update.
	public string ExecutablePath { get; init; } = "codex";

	public string Model { get; init; } = "gpt-5.6-sol";

	public string Sandbox { get; init; } = "read-only";

	public int TimeoutSeconds { get; init; } = 3600;
}
