#!/usr/bin/env bash
# test-hooks.sh — negative tests proving each hook fence with synthetic hook input.
# Self-contained: repo-state fixtures are scaffolded under a temp CLAUDE_PROJECT_DIR.
set -u
HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
LOG="$ROOT/protodog/logs/hook-denials.log"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
cd "$ROOT"
pass=0; fail=0

bash_json() { printf '{"tool_name":"Bash","tool_input":{"command":"%s"}}' "$1"; }
file_json() { printf '{"tool_name":"Write","tool_input":{"file_path":"%s"}}' "$1"; }

run() { echo "$2" | dotnet "$HERE/protodog-hooks.cs" "$1" >/dev/null 2>&1; }
expect_deny() { if run "$1" "$2"; then fail=$((fail+1)); echo "FAIL (expected deny): $3"; else pass=$((pass+1)); fi }
expect_allow() { if run "$1" "$2"; then pass=$((pass+1)); else fail=$((fail+1)); echo "FAIL (expected allow): $3"; fi }

LOG_BEFORE=$( [ -f "$LOG" ] && wc -l < "$LOG" || echo 0 )

# --- pre-bash: engineer-owned git operations are denied ----------------------
for c in "git push" "git push origin feature" "cd x && git push" "git fetch origin" "git pull" \
         "git rebase main" "git rebase -i HEAD~3" "git filter-branch --all" \
         "git commit --amend -m x" "git reset --hard HEAD~1" "git branch -D old" \
         "git checkout main" "git switch main" "git merge main" "git merge origin/main" \
         "git worktree remove ../wt"; do
  expect_deny pre-bash "$(bash_json "$c")" "pre-bash: $c"
done

# --- pre-bash: delegated worktree git is allowed -----------------------------
for c in "git status" "git diff" "git log --oneline" "git add -A && git commit -m checkpoint" \
         "git branch feature-x" "git merge feature-x" "git switch feature-x" "ls -la"; do
  expect_allow pre-bash "$(bash_json "$c")" "pre-bash: $c"
done

# --- pre-write: immutability against scaffolded repo fixtures ----------------
mkdir -p "$TMP/repo/audits" "$TMP/repo/specs" "$TMP/repo/plan/p"
touch "$TMP/repo/audits/audit-01.md"
printf 'bound spec\n' > "$TMP/repo/specs/bound.md"
printf 'unbound spec\n' > "$TMP/repo/specs/unbound.md"
printf -- '- Status: ready\n- Spec context:\n  - @specs/bound.md\n' > "$TMP/repo/plan/p/task-plan.md"
fx_run() { echo "$2" | env CLAUDE_PROJECT_DIR="$TMP/repo" PROTOCOL_DENIAL_LOG="$TMP/fx-denials.log" dotnet "$HERE/protodog-hooks.cs" "$1" >/dev/null 2>&1; }
fx_deny() { if fx_run "$1" "$2"; then fail=$((fail+1)); echo "FAIL (expected deny): $3"; else pass=$((pass+1)); fi }
fx_allow() { if fx_run "$1" "$2"; then pass=$((pass+1)); else fail=$((fail+1)); echo "FAIL (expected allow): $3"; fi }
fx_deny  pre-write "$(file_json "$TMP/repo/audits/audit-01.md")" "existing audit report is immutable"
fx_allow pre-write "$(file_json "$TMP/repo/audits/audit-02.md")" "next audit report may be created"
fx_deny  pre-write "$(file_json "$TMP/repo/specs/bound.md")" "bound spec is immutable"
fx_allow pre-write "$(file_json "$TMP/repo/specs/unbound.md")" "unbound spec is editable"

# --- post-plan-write: validator routing --------------------------------------
expect_allow post-plan-write "$(file_json "$ROOT/protodog/templates/task-plan.md")" "valid plan passes"
cp "$ROOT/protodog/templates/task-plan.md" "$TMP/task-plan.md"
sed -i '' 's/- Status: pending planning/- Status: vibing/' "$TMP/task-plan.md"
expect_deny  post-plan-write "$(file_json "$TMP/task-plan.md")" "invalid plan is rejected back"
expect_allow post-plan-write "$(file_json "$ROOT/README-nonexistent.txt")" "non-plan files ignored"

# --- plugin-mode resolution: env-provided roots and denial-log override ------
PLUGIN_LOG="$TMP/plugin-denials.log"
plugin_run() { echo "$2" | env CLAUDE_PLUGIN_ROOT="$ROOT/protodog" CLAUDE_PROJECT_DIR="$ROOT" PROTOCOL_DENIAL_LOG="$PLUGIN_LOG" dotnet "$HERE/protodog-hooks.cs" "$1" >/dev/null 2>&1; }
if plugin_run pre-bash "$(bash_json "git push")"; then fail=$((fail+1)); echo "FAIL (expected deny): plugin-mode git boundary"; else pass=$((pass+1)); fi
if plugin_run post-plan-write "$(file_json "$ROOT/protodog/templates/task-plan.md")"; then pass=$((pass+1)); else fail=$((fail+1)); echo "FAIL (expected allow): plugin-mode validator routing via CLAUDE_PLUGIN_ROOT"; fi
if [ -s "$PLUGIN_LOG" ]; then pass=$((pass+1)); else fail=$((fail+1)); echo "FAIL: denial-log override not honored"; fi

# --- repo-local denial log grew ----------------------------------------------
LOG_AFTER=$( [ -f "$LOG" ] && wc -l < "$LOG" || echo 0 )
if [ "$LOG_AFTER" -gt "$LOG_BEFORE" ]; then pass=$((pass+1)); else fail=$((fail+1)); echo "FAIL: denial log did not grow"; fi

echo "hook tests: $pass passed, $fail failed"
[ "$fail" -eq 0 ]
