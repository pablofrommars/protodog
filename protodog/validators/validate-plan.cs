#!/usr/bin/env dotnet
// validate-plan.cs — deterministic validator for Protodog artifacts.
// Usage: dotnet validate-plan.cs <path> ... where <path> is a plan/<plan-id> directory
// (whole tree, cross-references, audit-cycle filenames), an individual artifact file, or the
// deferred register (plan/deferred.md).
// Exit 0: valid. Exit 1: violations, one per line as "<file>: <message>". Exit 2: usage.
using System.Text.RegularExpressions;

string[] statuses = ["pending planning", "ready", "in progress", "blocked", "completed", "superseded", "cancelled"];
string[] cadences = ["interactive", "continuous"];

// Status markers, reused across row kinds: not started, active, done, needs attention, and
// closed without being met. Each is a single codepoint with no variation selector — a marker
// requiring U+FE0F would be rejected on an invisible difference after an ordinary copy-paste.
// The marker is the scannable summary; the label beside it remains the precise status.
string[] markers = ["⬜", "🟡", "✅", "🔴", "⬛"];
Dictionary<string, string> workMarkers = new()
{
	["pending planning"] = "⬜",
	["ready"] = "⬜",
	["in progress"] = "🟡",
	["blocked"] = "🔴",
	["completed"] = "✅",
	["superseded"] = "⬛",
	["cancelled"] = "⬛",
};
Dictionary<string, string> acceptanceMarkers = new()
{
	["pending"] = "⬜",
	["satisfied"] = "✅",
	["accepted exception"] = "⬛",
	["unmet"] = "🔴",
};
Dictionary<string, string> gateMarkers = new()
{
	["open"] = "🔴",
	["settled"] = "✅",
};
Dictionary<string, string> registerMarkers = new()
{
	["parked"] = "⬜",
	["closed"] = "✅",
	["dropped"] = "⬛",
};
string[] dispositions = ["actionable", "engineer's call", "trigger-blocked", "standing debt"];

// Alternation, never a character class: 🟡 and 🔴 are surrogate pairs in UTF-16, and a class
// would match their halves independently.
var workCell = new Regex($@"^({string.Join("|", markers)}) (~~)?([A-Z]+-\d{{2,}})(~~)? · (.+)$");
var auditFileName = new Regex(@"^audit-\d{2,}(-dispatch|-challenge)?\.md$");
var problems = new List<string>();

if (args.Length == 0)
{
	Console.Error.WriteLine("usage: validate-plan.cs <plan-dir-or-file> ...");
	Environment.Exit(2);
	return;
}

foreach (var arg in args)
{
	var path = Path.GetFullPath(arg);
	if (!Path.Exists(path))
	{
		Bad(path, "does not exist");
		continue;
	}

	if (Directory.Exists(path))
	{
		ValidateDirectory(path);
	}
	else
	{
		ValidateFile(path, null);
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

Console.WriteLine($"valid: {string.Join(", ", args)}");

void Bad(string file, string message)
{
	problems.Add($"{Path.GetRelativePath(Environment.CurrentDirectory, file)}: {message}");
}

bool[] FenceMask(string[] lines)
{
	// Fenced code blocks are opaque: nothing inside one is a section, table, or header line.
	var mask = new bool[lines.Length];
	var inFence = false;
	for (var i = 0; i < lines.Length; i++)
	{
		if (lines[i].TrimStart().StartsWith("```"))
		{
			inFence = !inFence;
			mask[i] = true;
			continue;
		}

		mask[i] = inFence;
	}

	return mask;
}

Dictionary<string, List<string>> ParseHeader(string[] lines)
{
	var header = new Dictionary<string, List<string>>();
	string? current = null;
	foreach (var line in lines)
	{
		if (line.StartsWith("## "))
		{
			break;
		}

		var keyed = Regex.Match(line, @"^- ([A-Z][A-Za-z ]*):\s*(.*)$");
		if (keyed.Success)
		{
			current = keyed.Groups[1].Value;
			header[current] = [keyed.Groups[2].Value.Trim()];
			continue;
		}

		var item = Regex.Match(line, @"^\s+- (.+)$");
		if (item.Success && current is not null)
		{
			header[current].Add(item.Groups[1].Value.Trim());
		}
	}

	return header;
}

string? First(Dictionary<string, List<string>> header, string key)
{
	return header.TryGetValue(key, out var values) ? values.FirstOrDefault(v => v.Length > 0) : null;
}

List<Section> Sections(string[] lines, bool[] mask)
{
	var sections = new List<Section>();
	for (var i = 0; i < lines.Length; i++)
	{
		if (mask[i])
		{
			continue;
		}

		var m = Regex.Match(lines[i], @"^## (.+)$");
		if (m.Success)
		{
			sections.Add(new Section(m.Groups[1].Value, i));
		}
	}

	for (var i = 0; i < sections.Count; i++)
	{
		sections[i].End = i + 1 < sections.Count ? sections[i + 1].Start : lines.Length;
	}

	return sections;
}

List<string> Cells(string line)
{
	var trimmed = line.Trim();
	if (trimmed.StartsWith('|'))
	{
		trimmed = trimmed[1..];
	}

	if (trimmed.EndsWith('|'))
	{
		trimmed = trimmed[..^1];
	}

	return [.. trimmed.Split('|').Select(c => c.Trim())];
}

Table? ParseTable(string[] lines, int start, int end, bool[] mask)
{
	for (var i = start; i < end; i++)
	{
		if (mask[i] || !lines[i].StartsWith('|'))
		{
			continue;
		}

		var header = Cells(lines[i]);
		var rows = new List<List<string>>();
		for (var j = i + 2; j < end && !mask[j] && lines[j].StartsWith('|'); j++)
		{
			rows.Add(Cells(lines[j]));
		}

		return new Table(header, rows);
	}

	return null;
}

List<string> Refs(string? cell, string pattern)
{
	if (cell is null || cell is "—" or "-")
	{
		return [];
	}

	return [.. Regex.Matches(cell, pattern).Select(m => m.Value)];
}

WorkCell? ParseWorkCell(string cell)
{
	// "⬜ STEP-01 · ready" / "✅ ~~GATE-01~~ · settled" — marker, id, status in one cell.
	var m = workCell.Match(cell);
	if (!m.Success || m.Groups[2].Value.Length != m.Groups[4].Value.Length)
	{
		return null;
	}

	return new WorkCell(m.Groups[1].Value, m.Groups[3].Value, m.Groups[5].Value.Trim(), m.Groups[2].Value.Length > 0);
}

// The marker map's keys are the row kind's status enum; its values are the required marker.
// strikeStatus names the one status whose identifier is struck, or null when none is.
WorkCell? CheckWorkCell(string file, string cell, string idPattern, Dictionary<string, string> allowed, string? strikeStatus)
{
	var parsed = ParseWorkCell(cell);
	if (parsed is null)
	{
		Bad(file, $"id cell \"{cell}\" must be \"<marker> <ID> · <status>\" with marker one of {string.Join(" ", markers)}");
		return null;
	}

	if (!Regex.IsMatch(parsed.Id, idPattern))
	{
		Bad(file, $"id \"{parsed.Id}\" malformed");
	}

	if (!allowed.TryGetValue(parsed.Status, out var expected))
	{
		Bad(file, $"status \"{parsed.Status}\" not in enum for \"{parsed.Id}\" (expected one of: {string.Join(", ", allowed.Keys)})");
		return parsed;
	}

	if (parsed.Marker != expected)
	{
		Bad(file, $"\"{parsed.Id}\" status \"{parsed.Status}\" requires marker \"{expected}\" (got \"{parsed.Marker}\")");
	}

	// Strikethrough marks a settled gate and nothing else.
	if (parsed.Struck != (strikeStatus is not null && parsed.Status == strikeStatus))
	{
		Bad(file, parsed.Struck
			? $"\"{parsed.Id}\" must not strike its id"
			: $"settled gate \"{parsed.Id}\" must strike its id (~~{parsed.Id}~~)");
	}

	return parsed;
}

void CheckContents(string file, string[] lines, List<Section> sections, bool[] mask)
{
	var contents = sections.FirstOrDefault(s => s.Title == "Contents");
	if (contents is null)
	{
		Bad(file, "missing \"## Contents\" section");
		return;
	}

	if (sections[0].Start != contents.Start)
	{
		Bad(file, "\"## Contents\" must be the first section");
	}

	var listed = new List<string>();
	for (var i = contents.Start + 1; i < contents.End; i++)
	{
		if (mask[i] || lines[i].Trim().Length == 0)
		{
			continue;
		}

		var entry = Regex.Match(lines[i], @"^- \[(.+)\]\(#[^)]*\)\s*$");
		if (entry.Success)
		{
			listed.Add(entry.Groups[1].Value.Trim());
		}
		else
		{
			Bad(file, $"contents entry \"{lines[i].Trim()}\" must be \"- [<section title>](#<anchor>)\"");
		}
	}

	var expected = sections.Where(s => s.Start != contents.Start).Select(s => s.Title).ToList();
	if (!listed.SequenceEqual(expected))
	{
		Bad(file, $"contents must list every section in document order [{string.Join(", ", expected)}] (got [{string.Join(", ", listed)}])");
	}
}

void CheckGates(string file, string[] lines, List<Section> sections, bool[] mask)
{
	var section = sections.FirstOrDefault(s => s.Title == "Gates");
	if (section is null)
	{
		return;
	}

	var table = CheckTable(file, lines, section, ["ID", "Gate", "Owner", "Blocked work", "Closure"], "Gates", mask);
	if (table is null)
	{
		return;
	}

	var seen = new HashSet<string>();
	foreach (var row in table.Rows)
	{
		if (row.Count < 5)
		{
			Bad(file, $"gates row \"{string.Join(" | ", row)}\" has {row.Count} cells, expected 5");
			continue;
		}

		var cell = CheckWorkCell(file, row[0], @"^GATE-\d{2,}$", gateMarkers, "settled");
		if (cell is not null && !seen.Add(cell.Id))
		{
			Bad(file, $"duplicate id \"{cell.Id}\"");
		}
	}
}

void CheckEmptySections(string file, string[] lines, bool[] mask)
{
	foreach (var section in Sections(lines, mask))
	{
		var body = string.Concat(lines[(section.Start + 1)..section.End]).Trim();
		if (body.Length == 0)
		{
			Bad(file, $"empty section \"## {section.Title}\"");
		}
	}
}

void CheckHeaderCommon(string file, Dictionary<string, List<string>> header, string? profile)
{
	if (profile is not null && First(header, "Profile") != profile)
	{
		Bad(file, $"Profile must be \"{profile}\" (got \"{First(header, "Profile") ?? ""}\")");
	}

	var status = First(header, "Status");
	if (status is null || !statuses.Contains(status))
	{
		Bad(file, $"Status \"{status ?? ""}\" not in status enum ({string.Join(" | ", statuses)})");
	}

	if (First(header, "Spec context") is null && First(header, "Grounding source") is null)
	{
		Bad(file, "neither \"Spec context\" nor \"Grounding source\" present");
	}
}

Table? CheckTable(string file, string[] lines, Section section, string[] expected, string label, bool[] mask)
{
	var table = ParseTable(lines, section.Start, section.End, mask);
	if (table is null)
	{
		Bad(file, $"section \"## {section.Title}\" has no table");
		return null;
	}

	if (!table.Header.SequenceEqual(expected))
	{
		Bad(file, $"{label} table columns must be [{string.Join(", ", expected)}] (got [{string.Join(", ", table.Header)}])");
	}

	return table;
}

HashSet<string>? CheckAcceptanceTable(string file, string[] lines, List<Section> sections, string title, bool[] mask)
{
	var section = sections.FirstOrDefault(s => s.Title == title);
	if (section is null)
	{
		return null;
	}

	var table = CheckTable(file, lines, section, ["Criterion", "Evidence"], title, mask);
	if (table is null)
	{
		return null;
	}

	var ids = new HashSet<string>();
	foreach (var row in table.Rows)
	{
		if (row.Count < 2)
		{
			Bad(file, $"acceptance row \"{string.Join(" | ", row)}\" has {row.Count} cells, expected 2");
			continue;
		}

		var cell = CheckWorkCell(file, row[0], @"^ACCEPTANCE-\d{2,}$", acceptanceMarkers, null);
		if (cell is not null && !ids.Add(cell.Id))
		{
			Bad(file, $"duplicate acceptance criterion \"{cell.Id}\"");
		}
	}

	return ids;
}

void CheckStepRows(string file, Table table, string idPattern, HashSet<string>? accIds, HashSet<string> seen)
{
	foreach (var row in table.Rows)
	{
		if (row.Count < 5)
		{
			Bad(file, $"step row \"{string.Join(" | ", row)}\" has {row.Count} cells, expected 5");
			continue;
		}

		var cell = CheckWorkCell(file, row[0], idPattern, workMarkers, null);
		if (cell is not null && !seen.Add(cell.Id))
		{
			Bad(file, $"duplicate id \"{cell.Id}\"");
		}

		if (accIds is not null)
		{
			foreach (var reference in Refs(row[3], @"ACCEPTANCE-\d{2,}"))
			{
				if (!accIds.Contains(reference))
				{
					Bad(file, $"\"{cell?.Id ?? row[0]}\" references unmapped {reference}");
				}
			}
		}
	}
}

// A terminal plan may not be the sole carrier of a live obligation: every top-level item in the
// section lifts to the deferred register or records its closure inline.
void CheckTerminalLift(string file, string[] lines, List<Section> sections, bool[] mask, string? status, string sectionTitle, string arrowPattern, string expectation)
{
	if (status is not ("completed" or "superseded" or "cancelled"))
	{
		return;
	}

	var section = sections.FirstOrDefault(s => s.Title == sectionTitle);
	if (section is null)
	{
		return;
	}

	for (var i = section.Start + 1; i < section.End; i++)
	{
		if (mask[i] || !lines[i].StartsWith("- "))
		{
			continue;
		}

		// An item may wrap: its continuation is every following non-blank line up to the next
		// top-level bullet. The arrow closes either the lead line (sub-bulleted ledger items) or
		// the item as a whole (wrapped text).
		var end = i + 1;
		while (end < section.End && !mask[end] && lines[end].Trim().Length > 0 && !lines[end].StartsWith("- "))
		{
			end++;
		}

		var joined = string.Join(" ", lines[i..end].Select(l => l.Trim()));
		if (!Regex.IsMatch(lines[i], arrowPattern) && !Regex.IsMatch(joined, arrowPattern))
		{
			var item = lines[i][2..];
			Bad(file, $"terminal plan: \"{(item.Length > 60 ? item[..60] + "…" : item)}\" in \"## {sectionTitle}\" must close with {expectation} at the end of its lead line or of the whole wrapped item");
		}

		i = end - 1;
	}
}

void ValidateDeferredRegister(string file)
{
	var lines = File.ReadAllLines(file);
	var mask = FenceMask(lines);
	var sections = Sections(lines, mask);
	CheckEmptySections(file, lines, mask);
	CheckContents(file, lines, sections, mask);
	var ids = new HashSet<string>();
	var parked = sections.FirstOrDefault(s => s.Title == "Parked");
	if (parked is null)
	{
		Bad(file, "missing \"## Parked\" section");
	}
	else
	{
		var table = CheckTable(file, lines, parked, ["ID", "Item", "Disposition", "Revisit", "Provenance"], "Parked", mask);
		if (table is not null)
		{
			foreach (var row in table.Rows)
			{
				if (row.Count < 5)
				{
					Bad(file, $"parked row \"{string.Join(" | ", row)}\" has {row.Count} cells, expected 5");
					continue;
				}

				var cell = CheckWorkCell(file, row[0], @"^D-\d{2,}$", registerMarkers, null);
				if (cell is not null)
				{
					if (!ids.Add(cell.Id))
					{
						Bad(file, $"duplicate id \"{cell.Id}\"");
					}

					if (cell.Status != "parked")
					{
						Bad(file, $"\"{cell.Id}\" under \"## Parked\" must be \"parked\" (got \"{cell.Status}\")");
					}
				}

				if (!dispositions.Contains(row[2]))
				{
					Bad(file, $"disposition \"{row[2]}\" not in enum for \"{cell?.Id ?? row[0]}\"");
				}
			}
		}
	}

	var closed = sections.FirstOrDefault(s => s.Title == "Closed");
	if (closed is null)
	{
		Bad(file, "missing \"## Closed\" section");
		return;
	}

	var closedTable = CheckTable(file, lines, closed, ["ID", "Outcome"], "Closed", mask);
	if (closedTable is null)
	{
		return;
	}

	foreach (var row in closedTable.Rows)
	{
		if (row.Count < 2)
		{
			Bad(file, $"closed row \"{string.Join(" | ", row)}\" has {row.Count} cells, expected 2");
			continue;
		}

		var cell = CheckWorkCell(file, row[0], @"^D-\d{2,}$", registerMarkers, null);
		if (cell is not null)
		{
			if (!ids.Add(cell.Id))
			{
				Bad(file, $"duplicate id \"{cell.Id}\"");
			}

			if (cell.Status == "parked")
			{
				Bad(file, $"\"{cell.Id}\" under \"## Closed\" cannot be \"parked\"");
			}
		}
	}
}

void ValidateTaskPlan(string file)
{
	var lines = File.ReadAllLines(file);
	var mask = FenceMask(lines);
	var header = ParseHeader(lines);
	var sections = Sections(lines, mask);
	CheckHeaderCommon(file, header, "Task");
	var cadence = First(header, "Cadence");
	if (cadence is null || !cadences.Contains(cadence))
	{
		Bad(file, $"Cadence \"{cadence ?? ""}\" not in enum ({string.Join(" | ", cadences)})");
	}

	CheckEmptySections(file, lines, mask);
	CheckContents(file, lines, sections, mask);
	CheckGates(file, lines, sections, mask);
	CheckTerminalLift(file, lines, sections, mask, First(header, "Status"), "Deferred issues and accepted gaps",
		@"→ (D-\d{2,}|closed: .+)$", "\"→ D-NN\" (lifted to plan/deferred.md) or \"→ closed: <reason>\"");
	var accIds = CheckAcceptanceTable(file, lines, sections, "Acceptance", mask) ?? [];
	var stepsSection = sections.FirstOrDefault(s => s.Title == "Steps");
	if (stepsSection is null)
	{
		Bad(file, "missing \"## Steps\" section");
		return;
	}

	var table = CheckTable(file, lines, stepsSection, ["ID", "Checkable result", "Affected surfaces", "Acceptance", "Verification"], "Steps", mask);
	var seen = new HashSet<string>();
	if (table is not null)
	{
		CheckStepRows(file, table, @"^STEP-\d{2,}$", accIds, seen);
	}

	foreach (var step in Refs(First(header, "Next boundary"), @"STEP-\d{2,}"))
	{
		if (!seen.Contains(step))
		{
			Bad(file, $"Next boundary references unknown {step}");
		}
	}
}

void ValidateProgramPlan(string file, string? planDir)
{
	var lines = File.ReadAllLines(file);
	var mask = FenceMask(lines);
	var header = ParseHeader(lines);
	var sections = Sections(lines, mask);
	CheckHeaderCommon(file, header, "Program");
	var cadence = First(header, "Cadence");
	if (cadence is null || !cadences.Contains(cadence))
	{
		Bad(file, $"Cadence \"{cadence ?? ""}\" not in enum ({string.Join(" | ", cadences)})");
	}

	CheckEmptySections(file, lines, mask);
	CheckContents(file, lines, sections, mask);
	CheckGates(file, lines, sections, mask);
	CheckTerminalLift(file, lines, sections, mask, First(header, "Status"), "Program issue ledger",
		@"→ (D-\d{2,}|closed: .+)$", "\"→ D-NN\" (lifted to plan/deferred.md) or \"→ closed: <reason>\"");
	var tracksSection = sections.FirstOrDefault(s => s.Title == "Tracks");
	if (tracksSection is null)
	{
		Bad(file, "missing \"## Tracks\" section");
		return;
	}

	var table = CheckTable(file, lines, tracksSection, ["Track", "Outcome", "Track plan", "Dependencies", "Acceptance"], "Tracks", mask);
	var ids = new HashSet<string>();
	if (table is not null)
	{
		foreach (var row in table.Rows)
		{
			if (row.Count < 5)
			{
				Bad(file, $"tracks row \"{string.Join(" | ", row)}\" has {row.Count} cells, expected 5");
				continue;
			}

			var cell = CheckWorkCell(file, row[0], @"^TRACK-\d{2,}$", workMarkers, null);
			var label = cell?.Id ?? row[0];
			if (cell is not null && !ids.Add(cell.Id))
			{
				Bad(file, $"duplicate track \"{cell.Id}\"");
			}

			if (row[2] != "not created" && !row[2].StartsWith('@'))
			{
				Bad(file, $"\"{label}\" Track plan cell must be \"not created\" or an @reference");
			}

			if (planDir is not null && row[2].StartsWith("@plan/"))
			{
				var root = Path.GetFullPath(Path.Combine(planDir, "..", ".."));
				if (!File.Exists(Path.Combine(root, row[2][1..])))
				{
					Bad(file, $"\"{label}\" track plan {row[2]} does not exist");
				}
			}
		}

		foreach (var row in table.Rows)
		{
			if (row.Count < 5)
			{
				continue;
			}

			foreach (var dependency in Refs(row[3], @"TRACK-\d{2,}"))
			{
				if (!ids.Contains(dependency))
				{
					Bad(file, $"\"{ParseWorkCell(row[0])?.Id ?? row[0]}\" depends on unknown {dependency}");
				}
			}
		}
	}

	CheckAcceptanceTable(file, lines, sections, "Program acceptance", mask);
	foreach (var track in Refs(First(header, "Next"), @"TRACK-\d{2,}"))
	{
		if (!ids.Contains(track))
		{
			Bad(file, $"Next references unknown {track}");
		}
	}
}

void ValidateTrackPlan(string file)
{
	var lines = File.ReadAllLines(file);
	var mask = FenceMask(lines);
	var header = ParseHeader(lines);
	var sections = Sections(lines, mask);
	var programPlan = First(header, "Program plan");
	if (programPlan is null || !programPlan.StartsWith('@'))
	{
		Bad(file, "missing @\"Program plan\" reference");
	}

	if (!Regex.IsMatch(First(header, "Track") ?? "", @"^TRACK-\d{2,}$"))
	{
		Bad(file, $"Track \"{First(header, "Track") ?? ""}\" malformed");
	}

	CheckHeaderCommon(file, header, null);
	var cadenceOverride = First(header, "Cadence override");
	if (cadenceOverride is not null && !cadences.Contains(cadenceOverride))
	{
		Bad(file, "Cadence override not in enum");
	}

	CheckEmptySections(file, lines, mask);
	CheckContents(file, lines, sections, mask);
	CheckGates(file, lines, sections, mask);
	CheckTerminalLift(file, lines, sections, mask, First(header, "Status"), "Deferred issues and accepted gaps",
		@"→ (ISSUE-\d{2,}|D-\d{2,}|closed: .+)$", "\"→ ISSUE-NN\" (deduplicated into the ledger), \"→ D-NN\", or \"→ closed: <reason>\"");
	var accIds = CheckAcceptanceTable(file, lines, sections, "Acceptance", mask) ?? [];
	var blocksSection = sections.FirstOrDefault(s => s.Title == "Blocks");
	if (blocksSection is null)
	{
		Bad(file, "missing \"## Blocks\" section");
		return;
	}

	var blocksTable = CheckTable(file, lines, blocksSection, ["Block", "Checkable result", "Dependencies", "Acceptance"], "Blocks", mask);
	var blockIds = new HashSet<string>();
	if (blocksTable is not null)
	{
		foreach (var row in blocksTable.Rows)
		{
			if (row.Count < 4)
			{
				Bad(file, $"blocks row \"{string.Join(" | ", row)}\" has {row.Count} cells, expected 4");
				continue;
			}

			var cell = CheckWorkCell(file, row[0], @"^BLOCK-\d{2,}$", workMarkers, null);
			if (cell is not null && !blockIds.Add(cell.Id))
			{
				Bad(file, $"duplicate block \"{cell.Id}\"");
			}
		}

		foreach (var row in blocksTable.Rows)
		{
			if (row.Count < 4)
			{
				continue;
			}

			foreach (var dependency in Refs(row[2], @"BLOCK-\d{2,}"))
			{
				if (!blockIds.Contains(dependency))
				{
					Bad(file, $"\"{ParseWorkCell(row[0])?.Id ?? row[0]}\" depends on unknown {dependency}");
				}
			}
		}
	}

	var seenSteps = new HashSet<string>();
	foreach (var section in sections.Where(s => Regex.IsMatch(s.Title, @"^BLOCK-\d{2,} — ")))
	{
		var blockId = section.Title.Split(" — ")[0];
		if (!blockIds.Contains(blockId))
		{
			Bad(file, $"detail section \"{section.Title}\" has no Blocks-table row");
		}

		var stepTable = CheckTable(file, lines, section, ["Step", "Checkable result", "Affected surfaces", "Acceptance", "Verification"], section.Title, mask);
		if (stepTable is not null)
		{
			CheckStepRows(file, stepTable, @"^STEP-\d{2,}$", accIds, seenSteps);
		}
	}

	foreach (var step in Refs(First(header, "Next boundary"), @"STEP-\d{2,}"))
	{
		if (!seenSteps.Contains(step))
		{
			Bad(file, $"Next boundary references unknown {step}");
		}
	}
}

void ValidateDispatch(string file)
{
	var lines = File.ReadAllLines(file);
	if (lines.Length == 0 || !Regex.IsMatch(lines[0], @"^# DISPATCH-\d{2,} — .+"))
	{
		Bad(file, "H1 must be \"# DISPATCH-NN — <checkable result>\"");
	}

	var header = ParseHeader(lines);
	string[] required =
	[
		"Program plan", "Track plan", "Assigned scope", "Checkable result", "Write boundaries",
		"Required verification", "Repository access", "Authority", "Stopping conditions", "Return path",
	];
	foreach (var key in required)
	{
		if (First(header, key) is null)
		{
			Bad(file, $"missing dispatch key \"{key}\"");
		}
	}

	var access = First(header, "Repository access") ?? "";
	if (access.Length > 0 && access != "read only" && !access.StartsWith("isolated writer — "))
	{
		Bad(file, "Repository access must be \"read only\" or \"isolated writer — <worktree>, <branch>, <baseline>\"");
	}
}

void ValidateSpec(string file)
{
	var lines = File.ReadAllLines(file);
	var mask = FenceMask(lines);
	if (!lines.Contains("## Why this document exists"))
	{
		Bad(file, "spec must open with \"## Why this document exists\"");
	}

	var sections = Sections(lines, mask);
	var criteria = sections.FirstOrDefault(s => s.Title == "Acceptance criteria");
	if (criteria is null)
	{
		return;
	}

	var table = ParseTable(lines, criteria.Start, criteria.End, mask);
	if (table is null)
	{
		return;
	}

	var ids = new HashSet<string>();
	foreach (var row in table.Rows)
	{
		if (!Regex.IsMatch(row[0], @"^ACCEPTANCE-\d{2,}$"))
		{
			Bad(file, $"criterion id \"{row[0]}\" must match ACCEPTANCE-NN");
		}

		if (!ids.Add(row[0]))
		{
			Bad(file, $"duplicate criterion \"{row[0]}\"");
		}
	}
}

void ValidateFile(string file, string? planDir)
{
	var baseName = Path.GetFileName(file);
	var separator = Path.DirectorySeparatorChar;
	switch (baseName)
	{
		case "task-plan.md":
		{
			ValidateTaskPlan(file);
			break;
		}
		case "program-plan.md":
		{
			ValidateProgramPlan(file, planDir);
			break;
		}
		case "track-plan.md":
		{
			ValidateTrackPlan(file);
			break;
		}
		case var name when Regex.IsMatch(name, @"^DISPATCH-\d+.*\.md$") || name == "dispatch.md":
		{
			ValidateDispatch(file);
			break;
		}
		case var name when name == "deferred.md" || name == "deferred-register.md":
		{
			ValidateDeferredRegister(file);
			break;
		}
		case var name when name == "spec.md" || file.Contains($"{separator}specs{separator}"):
		{
			ValidateSpec(file);
			break;
		}
		default:
		{
			Bad(file, "unrecognized artifact type (expected task-plan.md, program-plan.md, track-plan.md, DISPATCH-NN*.md, deferred.md, or a spec)");
			break;
		}
	}
}

void ValidateDirectory(string directory)
{
	var any = false;
	foreach (var file in Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories))
	{
		var baseName = Path.GetFileName(file);
		if (Path.GetFileName(Path.GetDirectoryName(file)) == "audits")
		{
			if (!auditFileName.IsMatch(baseName))
			{
				Bad(file, "audit artifact name must be audit-NN-dispatch.md, audit-NN.md, or audit-NN-challenge.md");
			}

			any = true;
			continue;
		}

		if (baseName is "task-plan.md" or "program-plan.md" or "track-plan.md" or "deferred.md" || Regex.IsMatch(baseName, @"^DISPATCH-\d+.*\.md$"))
		{
			ValidateFile(file, directory);
			any = true;
		}
	}

	if (!any)
	{
		Bad(directory, "no protocol artifacts found in directory");
	}
}

internal sealed record Table(List<string> Header, List<List<string>> Rows);

internal sealed record WorkCell(string Marker, string Id, string Status, bool Struck);

internal sealed record Section(string Title, int Start)
{
	public int End { get; set; }
}
