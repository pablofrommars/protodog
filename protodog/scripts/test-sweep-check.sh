#!/usr/bin/env bash
# test-sweep-check.sh — behavioral tests for sweep-check.cs against a scaffolded git repo.
set -u
HERE="$(cd "$(dirname "$0")" && pwd)"
V="dotnet $HERE/sweep-check.cs"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
pass=0; fail=0
has() { local n="$1" pat="$2"; if echo "$OUT" | grep -qF -- "$pat"; then pass=$((pass+1)); else
  fail=$((fail+1)); echo "FAIL: $n"; echo "$OUT" | sed 's/^/    /'; fi }

R="$TMP/repo"
mkdir -p "$R/plan/done-task" "$R/plan/live-task" "$R/plan/old-task" "$R/plan/dirty-task"
plan() { # path, status, optional deferred item line, optional spec reference
  { printf '# X\n\n- Profile: Task\n- Status: %s\n' "$2"
    [ -n "${4:-}" ] && printf -- '- Spec context: @%s\n' "$4"
    [ -n "${3:-}" ] && printf '\n## Deferred issues and accepted gaps\n\n- %s\n' "$3"
  } > "$1"
}
plan "$R/plan/done-task/task-plan.md" "completed" "parked thing → D-07" "specs/done.md"
plan "$R/plan/live-task/task-plan.md" "in progress" "" "specs/live.md"
plan "$R/plan/old-task/task-plan.md" "completed" "never lifted item"
plan "$R/plan/dirty-task/task-plan.md" "completed" "x → closed: done"
printf '# Deferred register\n\n| ⬜ D-07 · parked |\n' > "$R/plan/deferred.md"
printf 'notes\n' > "$R/plan/notes.md"
mkdir -p "$R/specs" "$R/inputs" "$R/docs" "$R/plan/gone-task"
printf 'spec\n' > "$R/specs/live.md"
printf 'spec\nfrom inputs/handoff.md\n' > "$R/specs/done.md"; printf 'handoff\n' > "$R/inputs/handoff.md"
printf 'spec\n' > "$R/specs/old.md";    plan "$R/plan/gone-task/task-plan.md" "completed" "" "specs/old.md"
printf 'idea\n' > "$R/inputs/idea.md"; printf 'see inputs/idea.md\n' > "$R/docs/note.md"
git init -q -b main "$R"
G() { git -C "$R" -c user.name=t -c user.email=t@t.t "$@"; }
G add -A; G commit -q -m base
G rm -q -r plan/gone-task; G commit -q -m "gone-task swept"   # specs/old.md was consumed, then its plan retired
G checkout -q -b work
mkdir -p "$R/plan/new-task"; plan "$R/plan/new-task/task-plan.md" "completed" "x → closed: done"
G add -A; G commit -q -m branch
printf 'stray\n' > "$R/plan/dirty-task/notes.md"   # untracked: invisible to git diff
printf 'draft\n' > "$R/inputs/draft.md"             # untracked input: nothing archives it
printf 'draft\n' > "$R/inputs/draft.md"             # untracked input: nothing archives it

OUT="$($V "$R" 2>&1)"
has "committed terminal plan is sweepable" "done-task/: sweepable"
has "live plan reported live"            "live-task/: live"
has "unlifted terminal plan blocked"     "old-task/: blocked — 1 unlifted"
has "branch-only terminal plan blocked"  "new-task/: blocked — not committed (branch-only)"
has "untracked content blocks sweep"     "dirty-task/: blocked — not committed (uncommitted working-tree content)"
has "register recognized with counts"    "deferred.md: register — 1 parked"
has "foreign file flagged"               "notes.md: foreign"
has "spec of a live plan is live"        "specs/live.md: live — cited by plan/live-task"
has "spec of a sweepable plan retires with it" "specs/done.md: retiring — with plan/done-task"
has "spec consumed then orphaned is retirable" "specs/old.md: retirable — consumed in history"
has "input cited by a retiring spec retires" "inputs/handoff.md: retiring — with specs/done.md"
has "never-consumed input is backlog"    "inputs/idea.md: backlog — never consumed by a plan"
has "docs citation into inputs is a defect" "defect: cited by docs/note.md"
has "uncommitted input is blocked"       "inputs/draft.md: blocked — cited by nothing; not committed"
has "summary counts one sweepable"       "survey complete: 1 mechanically sweepable, 1 spec/input file(s) retirable, 1 backlog"

OUT="$($V "$R" --base nope 2>&1)"
has "missing base ref disclosed"         'ref "nope" not found'

echo "sweep-check tests: $pass passed, $fail failed"
[ "$fail" -eq 0 ]
