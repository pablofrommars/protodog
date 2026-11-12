---
name: squash-message
description: Propose the one-line title for squashing this branch, in the engineer's own style — confirm the first commit's subject against the work done, or revise it when the work drifted. Engineer-invoked shortcut; proposes text only and never squashes or commits.
disable-model-invocation: true
argument-hint: [optional hint about the intent]
---

# Squash message

Propose the title for the `squash` or `merge` prompt: one line for everything this branch adds
past its base. Proposing is all this does: squash, rewrite, and landing stay where the Git policy
puts them. Staged changes are not part of a squash; commit them first, with `commit-message`.

Hint from the invocation, if any: $ARGUMENTS

## Branch state

Injected on Claude Code; on Codex, run the same commands first.

Commits past the base (`origin/HEAD`, else `main`), oldest first:

```!
git log --reverse --format=%s "$(git rev-parse --abbrev-ref origin/HEAD 2>/dev/null || echo main)..HEAD" 2>/dev/null || true
```

```!
git diff --stat "$(git rev-parse --abbrev-ref origin/HEAD 2>/dev/null || echo main)...HEAD" 2>/dev/null | tail -40 || true
```

Plan files this branch touches:

```!
{ git diff --name-only "$(git rev-parse --abbrev-ref origin/HEAD 2>/dev/null || echo main)...HEAD" -- plan/ inputs/; git ls-files --others --exclude-standard -- plan/ inputs/; } 2>/dev/null | sort -u || true
```

Uncommitted changes:

```!
git status --short 2>/dev/null | head -20 || true
```

```!
git log --format=%s -n 12 "$(git rev-parse --abbrev-ref origin/HEAD 2>/dev/null || echo main)" 2>/dev/null || true
```

## Keep the first subject unless the work drifted

The first commit's subject was written forward, naming what the work does; it is the default the
squash prompt offers. Check it against the planned work (this session's Task or runbook, else the
plan files above: a Task plan's label and acceptance; on a line worktree, the runbook's objective)
and against what the commits and diffstat deliver.

- It still holds: return it unchanged. A better phrasing is not a reason to change it.
- The delivered work no longer matches it — scope grew, narrowed, or moved — or it describes a
  step rather than the work: propose the revision and say in one sentence what changed.

## Style

The engineer's style, read from the repositories: sentence case, a leading verb or a noun phrase
("Add guarded pull workflow", "Harden FileDropZone after audit", "Readability fixes"), no type
prefix, no scope prefix, no trailing period, no body, no trailer. At most fifty characters. A
release reads `Release <version>: <result>`, the one form that takes a colon.

## Rules

- No commits past the base: say so in one line and stop.
- Uncommitted changes: note in one sentence that the squash will not include them.
- Never exceed fifty characters; outside a release, one result, no list.
- Return the line alone in a code block, with at most the sentences the rules above call for. Never pad with alternatives.
