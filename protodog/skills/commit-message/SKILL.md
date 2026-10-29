---
name: commit-message
description: Propose one concise commit message line for the staged changes, in the engineer's own style. Engineer-invoked shortcut; proposes text only and never commits.
disable-model-invocation: true
argument-hint: [optional hint about the intent]
---

# Commit message

Propose one line for the staged changes. Proposing is all this does: staging, committing, and
amending stay where the Git policy puts them.

Hint from the invocation, if any: $ARGUMENTS

## Staged changes

Injected on Claude Code; on Codex, run the same commands first.

```!
git diff --cached --stat 2>/dev/null || true
```

```!
git diff --cached -U0 2>/dev/null | head -300 || true
```

```!
git log --format=%s -n 12 2>/dev/null || true
```

## Style

The engineer's style, read from the repositories: sentence case, a leading verb or a noun phrase
("Add guarded pull workflow", "Harden FileDropZone after audit", "Readability fixes"), no type
prefix, no scope prefix, no trailing period, no body, no trailer. Usually under forty characters.
When one commit lands several results, the subject names the unit and lists them after a colon,
comma-separated, as in "Release 4.0.1: session-grounded runbooks" or "Light scorecard retries:
state the evidence bound, carry earlier feedback across fresh attempts". A release commit opens
with `Release <version>:`.

## Rules

- Nothing staged: say so in one line and stop. Do not fall back to the working tree.
- Describe the verified result, not the activity: what is true after the commit that was not before.
- Prefer the short form; use the colon-and-list form only when the diff genuinely lands several results, and keep the list to what matters.
- Return the line alone in a code block. Add at most one sentence, only if the diff mixes two unrelated units, naming the split. Never pad with alternatives.
