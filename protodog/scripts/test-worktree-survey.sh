#!/usr/bin/env bash
# test-worktree-survey.sh — behavioral tests for worktree-survey.cs against a scaffolded git repo.
set -u
HERE="$(cd "$(dirname "$0")" && pwd)"
V="dotnet $HERE/worktree-survey.cs"
TMP="$(mktemp -d)" || { echo "cannot create a temporary directory"; exit 2; }
TMP="$(cd "$TMP" && pwd -P)"   # the survey prints physical paths
trap 'rm -rf "$TMP"' EXIT
pass=0; fail=0
has() { local n="$1" pat="$2"; if echo "$OUT" | grep -qF -- "$pat"; then pass=$((pass+1)); else
  fail=$((fail+1)); echo "FAIL: $n"; echo "$OUT" | sed 's/^/    /'; fi }
lacks() { local n="$1" pat="$2"; if echo "$OUT" | grep -qF -- "$pat"; then
  fail=$((fail+1)); echo "FAIL: $n"; echo "$OUT" | sed 's/^/    /'; else pass=$((pass+1)); fi }

R="$TMP/repo"; W="$TMP/wt"
mkdir -p "$R/plan/merged-work" "$W"
printf 'base\n' > "$R/file.txt"
printf '# plan\n' > "$R/plan/merged-work/task-plan.md"
git init -q -b main "$R"
G() { git -C "$R" -c user.name=t -c user.email=t@t.t "$@"; }
G add -A; G commit -q -m base
BASE="$(G rev-parse HEAD)"

# merged-work: branch at main, nothing ahead, clean; its name matches a plan directory at main.
G worktree add -q -b merged-work "$W/merged-work" main
# squash-landed: one commit on the branch, squash-merged into main.
G worktree add -q -b squash-landed "$W/squash-landed" main
printf 'squashed\n' > "$W/squash-landed/squashed.txt"
git -C "$W/squash-landed" -c user.name=t -c user.email=t@t.t add -A
git -C "$W/squash-landed" -c user.name=t -c user.email=t@t.t commit -q -m "squash me"
G merge -q --squash squash-landed >/dev/null; G commit -q -m "landed squash"
# squash-two: two commits on the branch, landed as one squash commit.
G worktree add -q -b squash-two "$W/squash-two" main
printf 'one\n' > "$W/squash-two/one.txt"
git -C "$W/squash-two" -c user.name=t -c user.email=t@t.t add -A
git -C "$W/squash-two" -c user.name=t -c user.email=t@t.t commit -q -m "one"
printf 'two\n' > "$W/squash-two/two.txt"
git -C "$W/squash-two" -c user.name=t -c user.email=t@t.t add -A
git -C "$W/squash-two" -c user.name=t -c user.email=t@t.t commit -q -m "two"
G merge -q --squash squash-two >/dev/null; G commit -q -m "landed two as one"
# live-work: one commit main does not have.
G worktree add -q -b live-work "$W/live-work" main
printf 'live\n' > "$W/live-work/live.txt"
git -C "$W/live-work" -c user.name=t -c user.email=t@t.t add -A
git -C "$W/live-work" -c user.name=t -c user.email=t@t.t commit -q -m "live"
# dirty-work: merged but carrying an untracked file.
G worktree add -q -b dirty-work "$W/dirty-work" main
printf 'stray\n' > "$W/dirty-work/stray.txt"
# detached-old: detached at the base commit with a local commit on top.
G worktree add -q --detach "$W/detached-old" "$BASE"
printf 'detached\n' > "$W/detached-old/detached.txt"
git -C "$W/detached-old" -c user.name=t -c user.email=t@t.t add -A
git -C "$W/detached-old" -c user.name=t -c user.email=t@t.t commit -q -m "detached"
# one repository-wide stash
printf 'stash me\n' > "$R/file.txt"; G stash -q

OUT="$($V "$R" 2>&1)"
has "merged ancestor is a candidate"            "merged-work: candidate — merged (ancestor), clean"
has "candidate prints the worktree removal"     "remove: git worktree remove '$W/merged-work'; git branch -d 'merged-work'"
has "plan directory at base is a note"          "merged-work: candidate"; has "plan note text" "plan/merged-work still present at main"
has "unpublished branch is a note"              "branch never published"
has "squash landing is patch-equivalent"        "squash-landed: candidate — merged (patch-equivalent), clean"
has "patch-equivalent names -D with the reason" "branch -d would refuse a squash landing; -D is printed; remove: git worktree remove '$W/squash-landed'; git branch -D 'squash-landed'"
has "multi-commit squash landing is a candidate" "squash-two: candidate — merged (squash-landed), clean"
has "live branch is blocked ahead"              "live-work: blocked — ahead 1 commit(s)"
has "dirty merged worktree is blocked"          "dirty-work: blocked — dirty 1 path(s): ?? stray.txt"
has "detached worktree blocked with the note"   "detached-old: blocked — ahead 1 commit(s); note: detached HEAD"
has "stashes counted once"                      "stashes: 1 (repository-wide"
has "summary counts three candidates"           "survey complete: 3 candidate(s) of 6 worktree(s); nothing was removed"
lacks "no removal command for a blocked entry"  "git worktree remove '$W/live-work'"
[ -d "$W/merged-work" ] && pass=$((pass+1)) || { fail=$((fail+1)); echo "FAIL: survey removed a worktree"; }

OUT="$($V "$R" --base nope 2>&1)"
has "missing base ref disclosed"                'ref "nope" not found'
has "unknown merged state blocks"               "merged-work: blocked — merged state unknown"

echo "worktree-survey tests: $pass passed, $fail failed"
[ "$fail" -eq 0 ]
