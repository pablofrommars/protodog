---
name: commit-message
description: Propose one concise commit message line for the staged changes, in the engineer's own style — forward-looking for a branch's first commit, naming what the work does. Engineer-invoked shortcut; proposes text only and never commits.
disable-model-invocation: true
argument-hint: [optional hint about the intent]
---

# Commit message

Propose one line for the staged changes. Proposing is all this does: staging, committing, and
amending stay where the Git policy puts them. At the `squash` or `merge` prompt, the title comes
from `squash-message` instead.

Hint from the invocation, if any: $ARGUMENTS

## Repository state

Injected on Claude Code; on Codex, run the same commands first.

```!
git diff --cached --stat 2>/dev/null || true
```

```!
git diff --cached -U0 2>/dev/null | head -300 || true
```

Commits on this branch past its base (`origin/HEAD`, else `main`):

```!
git log --format=%s "$(git rev-parse --abbrev-ref origin/HEAD 2>/dev/null || echo main)..HEAD" 2>/dev/null || true
```

Plan files this branch touches:

```!
{ git diff --name-only "$(git rev-parse --abbrev-ref origin/HEAD 2>/dev/null || echo main)" -- plan/ inputs/; git ls-files --others --exclude-standard -- plan/ inputs/; } 2>/dev/null | sort -u || true
```

```!
git log --format=%s -n 12 2>/dev/null || true
```

## The first commit looks forward

When the branch has no commits past its base, this commit's subject is the provisional
integration title: squash workflows offer it as the default and it usually lands unchanged. Name
what the work does, not the step being staged. Draw it from the planned work: this session's Task
or runbook unit, else the plan files above (a Task plan's label and acceptance; on a line
worktree, the runbook's objective, since the line lands once). The staged diff only confirms the
work is under way. With no plan in the session or the files, describe the staged result and say
in one sentence that no plan was found.

Every later commit describes its own staged result; squashing folds it away.

## Style

The engineer's style, read from the repositories: sentence case, a leading verb or a noun phrase
("Add guarded pull workflow", "Harden FileDropZone after audit", "Readability fixes"), no type
prefix, no scope prefix, no trailing period, no body, no trailer. At most fifty characters. A
release commit reads `Release <version>: <result>`, the one form that takes a colon, as in
"Release 4.0.1: session-grounded runbooks".

## Rules

- Nothing staged: say so in one line and stop. Do not fall back to the working tree.
- Describe the result, not the activity: what is true after the work that was not before.
- Never exceed fifty characters; outside a release, one result, no list.
- Return the line alone in a code block. Add at most one sentence, only when the staged diff mixes two unrelated units (name the split) or no plan was found. Never pad with alternatives.
