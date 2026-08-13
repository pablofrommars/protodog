#!/usr/bin/env dotnet
// validate-registry.cs — deterministic validator for the prompt snippet registries.
// Usage: dotnet validate-registry.cs <file.code-snippets> ...
// Checks: JSONC parseability, required fields, scope, unique keys and prefixes across ALL
// given files, tab-stop ordering and numbering, and mini-protocol family coherence
// (p-protocol-* prefix <-> "mini protocol" description).
using System.Text.Json;
using System.Text.RegularExpressions;

var problems = new List<string>();
var keys = new Dictionary<string, List<string>>();
var prefixes = new Dictionary<string, List<string>>();
var options = new JsonDocumentOptions
{
	CommentHandling = JsonCommentHandling.Skip,
	AllowTrailingCommas = true,
};

if (args.Length == 0)
{
	Console.Error.WriteLine("usage: validate-registry.cs <file.code-snippets> ...");
	Environment.Exit(2);
	return;
}

foreach (var file in args)
{
	if (!File.Exists(file))
	{
		Bad(file, "does not exist");
		continue;
	}

	JsonDocument document;
	try
	{
		document = JsonDocument.Parse(File.ReadAllText(file), options);
	}
	catch (JsonException e)
	{
		Bad(file, $"parse failure: {e.Message}");
		continue;
	}

	using (document)
	{
		foreach (var property in document.RootElement.EnumerateObject())
		{
			var key = property.Name;
			var snippet = property.Value;
			var location = $"\"{key}\"";
			foreach (var field in (string[])["scope", "prefix", "body", "description"])
			{
				if (!snippet.TryGetProperty(field, out _))
				{
					Bad(file, $"{location} missing {field}");
				}
			}

			if (snippet.TryGetProperty("scope", out var scope) && scope.GetString() != "markdown")
			{
				Bad(file, $"{location} scope must be \"markdown\"");
			}

			var prefix = snippet.TryGetProperty("prefix", out var prefixElement) ? prefixElement.GetString() ?? "" : "";
			var description = snippet.TryGetProperty("description", out var descriptionElement) ? descriptionElement.GetString() ?? "" : "";
			var familyPrefix = prefix.StartsWith("p-protocol-");
			var familyDescription = description.Contains("mini protocol", StringComparison.OrdinalIgnoreCase);
			if (familyPrefix != familyDescription)
			{
				Bad(file, familyPrefix
					? $"{location} prefix \"{prefix}\" marks the mini-protocol family; description must say \"mini protocol\""
					: $"{location} description claims a mini protocol; prefix must start with \"p-protocol-\"");
			}

			Track(keys, key, file);
			if (prefix.Length > 0)
			{
				Track(prefixes, prefix, $"{file} {location}");
			}

			if (!snippet.TryGetProperty("body", out var body) || body.ValueKind != JsonValueKind.Array)
			{
				Bad(file, $"{location} body must be an array");
				continue;
			}

			var text = string.Join("\n", body.EnumerateArray().Select(e => e.GetString() ?? ""));
			var seen = new HashSet<int>();
			var order = new List<int>();
			foreach (Match match in Regex.Matches(text, @"\$(\d+)|\$\{(\d+)[:|}]"))
			{
				var value = match.Groups[1].Success ? match.Groups[1].Value : match.Groups[2].Value;
				var number = int.Parse(value);
				if (number != 0 && seen.Add(number))
				{
					order.Add(number);
				}
			}

			for (var i = 1; i < order.Count; i++)
			{
				if (order[i] < order[i - 1])
				{
					Bad(file, $"{location} tab stops out of fill order: {string.Join(",", order)}");
					break;
				}
			}

			var sorted = seen.Order().ToList();
			for (var i = 0; i < sorted.Count; i++)
			{
				if (sorted[i] != i + 1)
				{
					Bad(file, $"{location} tab stops not contiguous from 1: {string.Join(",", sorted)}");
					break;
				}
			}
		}
	}
}

foreach (var (key, files) in keys)
{
	if (files.Count > 1)
	{
		Bad(files[0], $"duplicate snippet key \"{key}\"");
	}
}

foreach (var (prefix, locations) in prefixes)
{
	if (locations.Count > 1)
	{
		Bad(locations[0].Split(' ')[0], $"duplicate prefix \"{prefix}\"");
	}
}

if (problems.Count > 0)
{
	foreach (var problem in problems)
	{
		Console.Error.WriteLine(problem);
	}

	Environment.Exit(1);
}

Console.WriteLine($"valid: {string.Join(", ", args.Select(Path.GetFileName))}");

void Bad(string file, string message)
{
	problems.Add($"{Path.GetRelativePath(Environment.CurrentDirectory, file)}: {message}");
}

void Track(Dictionary<string, List<string>> map, string key, string location)
{
	if (!map.TryGetValue(key, out var list))
	{
		list = [];
		map[key] = list;
	}

	list.Add(location);
}
