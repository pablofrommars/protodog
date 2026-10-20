#!/usr/bin/env dotnet
// Builds a self-contained Task-only local marketplace from the shared protocol sources.
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Nodes;

if (args is not [var destination])
{
	Console.Error.WriteLine("usage: build-codex-package.cs <new-marketplace-directory>");
	return 2;
}

var source = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(SourceFile())!, ".."));
var output = Path.GetFullPath(destination);
if (Path.Exists(output) || output.StartsWith(source + Path.DirectorySeparatorChar))
{
	Console.Error.WriteLine("REFUSED: output must be a new directory outside the protodog source package");
	return 2;
}

var package = Path.Combine(output, "protodog");
Directory.CreateDirectory(package);
foreach (var directory in new[] { "core", "templates" })
{
	CopyTree(Path.Combine(source, directory), Path.Combine(package, directory));
}

foreach (var file in new[]
{
	"validators/validate-plan.cs", "hooks/hooks.json", "hooks/protodog-hooks.cs",
	"scripts/audit-launch.cs", "scripts/sweep-check.cs", "agents/challenge-codex.toml",
})
{
	var target = Path.Combine(package, file);
	Directory.CreateDirectory(Path.GetDirectoryName(target)!);
	File.Copy(Path.Combine(source, file), target);
}

File.Copy(Path.Combine(source, "..", "README.md"), Path.Combine(package, "README.md"));

foreach (var skill in new[] { "task", "plan-sweep", "deferred-review", "working-canvas" })
{
	CopyTree(Path.Combine(source, "skills", skill), Path.Combine(package, "skills", skill));
}

var version = JsonNode.Parse(File.ReadAllText(Path.Combine(source, ".claude-plugin", "plugin.json")))!["version"]!.GetValue<string>();
var options = new JsonSerializerOptions { WriteIndented = true };
var manifest = new JsonObject
{
	["$schema"] = "https://agent-plugins.org/schemas/1.0.0/plugin.schema.json",
	["name"] = "protodog",
	["version"] = version,
	["description"] = "Protodog Task protocol for local Codex: shared plans, enforcement hooks, maintenance, and independent Claude audits. Program is not supported.",
	["author"] = new JsonObject { ["name"] = "Pablo Belin" },
	["license"] = "Proprietary",
	["extensions"] = new JsonObject
	{
		["com.openai"] = new JsonObject { ["hooks"] = "./hooks/hooks.json" },
	},
};
File.WriteAllText(Path.Combine(package, "plugin.json"), manifest.ToJsonString(options) + "\n");

var marketplace = Path.Combine(output, ".agents", "plugins");
Directory.CreateDirectory(marketplace);
var catalog = new JsonObject
{
	["name"] = "kennel-codex",
	["plugins"] = new JsonArray
	{
		new JsonObject
		{
			["name"] = "protodog",
			["source"] = new JsonObject { ["source"] = "local", ["path"] = "./protodog" },
			["policy"] = new JsonObject { ["installation"] = "AVAILABLE", ["authentication"] = "ON_INSTALL" },
			["category"] = "Productivity",
		},
	},
};
File.WriteAllText(Path.Combine(marketplace, "marketplace.json"), catalog.ToJsonString(options) + "\n");
Console.WriteLine($"Built Task-only marketplace: {output}");
Console.WriteLine("Register this directory with codex plugin marketplace add; install protodog@kennel-codex, then review /hooks.");
return 0;

void CopyTree(string from, string to)
{
	Directory.CreateDirectory(to);
	foreach (var file in Directory.EnumerateFiles(from))
	{
		File.Copy(file, Path.Combine(to, Path.GetFileName(file)));
	}

	foreach (var directory in Directory.EnumerateDirectories(from))
	{
		CopyTree(directory, Path.Combine(to, Path.GetFileName(directory)));
	}
}

string SourceFile([CallerFilePath] string path = "")
{
	return path;
}
