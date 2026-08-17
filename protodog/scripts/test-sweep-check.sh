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
mkdir -p "$R/plan/done-task" "$R/plan/live-task" "$R/plan/old-task"
plan() { # path, status, optional deferred item line
  { printf '# X\n\n- Profile: Task\n- Status: %s\n' "$2"
    [ -n "${3:-}" ] && printf '\n## Deferred issues and accepted gaps\n\n- %s\n' "$3"
  } > "$1"
}
plan "$R/plan/done-task/task-plan.md" "completed" "parked thing → D-07"
plan "$R/plan/live-task/task-plan.md" "in progress"
plan "$R/plan/old-task/task-plan.md" "completed" "never lifted item"
printf '# Deferred register\n\n| ⬜ D-07 · parked |\n' > "$R/plan/deferred.md"
printf 'notes\n' > "$R/plan/notes.md"
git init -q -b main "$R"
G() { git -C "$R" -c user.name=t -c user.email=t@t.t "$@"; }
G add -A; G commit -q -m base
G checkout -q -b work
mkdir -p "$R/plan/new-task"; plan "$R/plan/new-task/task-plan.md" "completed" "x → closed: done"
G add -A; G commit -q -m branch

OUT="$($V "$R" 2>&1)"
has "committed terminal plan is sweepable" "done-task/: sweepable"
has "live plan reported live"            "live-task/: live"
has "unlifted terminal plan blocked"     "old-task/: blocked — 1 unlifted"
has "branch-only terminal plan blocked"  "new-task/: blocked — not committed (branch-only)"
has "register recognized with counts"    "deferred.md: register — 1 parked"
has "foreign file flagged"               "notes.md: foreign"
has "summary counts one sweepable"       "survey complete: 1 mechanically sweepable"

OUT="$($V "$R" --base nope 2>&1)"
has "missing base ref disclosed"         'ref "nope" not found'

echo "sweep-check tests: $pass passed, $fail failed"
[ "$fail" -eq 0 ]
