#!/usr/bin/env bash
# test-validators.sh — behavioral tests for validate-plan.js.
# Positive: templates and this repository's live plans validate clean.
# Negative (fail-first evidence): every check class rejects a seeded violation.
set -u
HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
V="dotnet $HERE/validate-plan.cs"
T="$ROOT/protodog/templates"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
pass=0; fail=0

expect_ok() { # name, args...
  local name="$1"; shift
  if $V "$@" >/dev/null 2>&1; then pass=$((pass+1)); else
    fail=$((fail+1)); echo "FAIL (expected valid): $name"; $V "$@" 2>&1 | sed 's/^/    /'; fi
}
expect_bad() { # name, args...
  local name="$1"; shift
  if $V "$@" >/dev/null 2>&1; then fail=$((fail+1)); echo "FAIL (expected rejection): $name"
  else pass=$((pass+1)); fi
}

# --- positive ---------------------------------------------------------------
expect_ok "template task plan"    "$T/task-plan.md"
expect_ok "template program plan" "$T/program-plan.md"
expect_ok "template track plan"   "$T/track-plan.md"
expect_ok "template dispatch"     "$T/dispatch.md"
expect_ok "template spec"         "$T/spec.md"

# --- negative: one seeded violation per check class -------------------------
seed() { cp "$T/task-plan.md" "$TMP/task-plan.md"; }

seed; sed -i '' 's/- Status: pending planning/- Status: doing stuff/' "$TMP/task-plan.md"
expect_bad "status outside enum" "$TMP/task-plan.md"

seed; sed -i '' 's/- Cadence: interactive/- Cadence: collaborative/' "$TMP/task-plan.md"
expect_bad "retired cadence term" "$TMP/task-plan.md"

seed; sed -i '' '/- Cadence: interactive/d' "$TMP/task-plan.md"
expect_bad "missing header field" "$TMP/task-plan.md"

# Duplicate the step row in place, inside the Steps table — appending past the acceptance
# table would instead be read as a malformed acceptance row.
seed; awk '{print} /^\| - \[ \] STEP-01/{print}' "$TMP/task-plan.md" > "$TMP/dup.md"; mv "$TMP/dup.md" "$TMP/task-plan.md"
expect_bad "duplicate step id" "$TMP/task-plan.md"

seed; sed -i '' 's/| ACCEPTANCE-01 | <command/| ACCEPTANCE-99 | <command/' "$TMP/task-plan.md"
expect_bad "unmapped acceptance reference" "$TMP/task-plan.md"

seed; printf '\n## Gates\n' >> "$TMP/task-plan.md"; sed -i '' 's|^- \[Acceptance\](#acceptance)$|&\
- [Gates](#gates)|' "$TMP/task-plan.md"
expect_bad "empty optional section" "$TMP/task-plan.md"

seed; sed -i '' 's/- Next boundary: STEP-01/- Next boundary: STEP-07/' "$TMP/task-plan.md"
expect_bad "next boundary references unknown step" "$TMP/task-plan.md"

# --- negative: id cell (checkbox + id + status) ------------------------------
seed; sed -i '' 's/| - \[ \] STEP-01 · ready |/| STEP-01 |/' "$TMP/task-plan.md"
expect_bad "bare id without checkbox and status" "$TMP/task-plan.md"

seed; sed -i '' 's/- \[ \] STEP-01 · ready/- [ ] STEP-01/' "$TMP/task-plan.md"
expect_bad "checkbox without status label" "$TMP/task-plan.md"

seed; sed -i '' 's/- \[ \] STEP-01 · ready/- [x] STEP-01 · ready/' "$TMP/task-plan.md"
expect_bad "checked box on non-terminal status" "$TMP/task-plan.md"

seed; sed -i '' 's/- \[ \] STEP-01 · ready/- [ ] STEP-01 · completed/' "$TMP/task-plan.md"
expect_bad "unchecked box on terminal status" "$TMP/task-plan.md"

seed; sed -i '' 's/- \[ \] STEP-01 · ready/- [ ] STEP-01 · doing stuff/' "$TMP/task-plan.md"
expect_bad "status outside enum inside id cell" "$TMP/task-plan.md"

seed; sed -i '' 's/- \[ \] STEP-01 · ready/- [ ] ~~STEP-01~~ · ready/' "$TMP/task-plan.md"
expect_bad "strikethrough on a step id" "$TMP/task-plan.md"

# --- negative: contents ------------------------------------------------------
seed; sed -i '' '/^- \[Acceptance\](#acceptance)$/d' "$TMP/task-plan.md"
expect_bad "contents omits a section" "$TMP/task-plan.md"

seed; sed -i '' 's|^- \[Steps\](#steps)$|- [Steps](#steps)\
- [Phantom](#phantom)|' "$TMP/task-plan.md"
expect_bad "contents lists a section that does not exist" "$TMP/task-plan.md"

seed; sed -i '' 's|^- \[Steps\](#steps)$|Steps|' "$TMP/task-plan.md"
expect_bad "contents entry is not a link" "$TMP/task-plan.md"

seed; sed -i '' '/^## Contents$/,/^## Steps$/{/^## Steps$/!d;}' "$TMP/task-plan.md"
expect_bad "missing contents section" "$TMP/task-plan.md"

cp "$T/dispatch.md" "$TMP/DISPATCH-01.md"; sed -i '' '/- Return path:/d' "$TMP/DISPATCH-01.md"
expect_bad "dispatch missing required key" "$TMP/DISPATCH-01.md"

cp "$T/dispatch.md" "$TMP/DISPATCH-02.md"
sed -i '' 's/- Repository access: read only/- Repository access: coordination writer — worktree, branch/' "$TMP/DISPATCH-02.md"
expect_bad "removed coordination-writer access mode" "$TMP/DISPATCH-02.md"

mkdir -p "$TMP/plan-x/audits"; cp "$T/task-plan.md" "$TMP/plan-x/task-plan.md"
touch "$TMP/plan-x/audits/audit-1-report.md"
expect_bad "audit-cycle filename pattern" "$TMP/plan-x"

# --- gates: strike marks a settled gate, and nothing else --------------------
gated() { # id-cell -> a task plan carrying one gate
  cat > "$TMP/task-plan.md" <<EOF
# Gate fixture

- Profile: Task
- Status: in progress
- Cadence: interactive
- Spec context:
  - @specs/x.md
- Next boundary: STEP-01

## Contents

- [Steps](#steps)
- [Gates](#gates)
- [Acceptance](#acceptance)

## Steps

| ID | Checkable result | Affected surfaces | Acceptance | Verification |
|---|---|---|---|---|
| - [ ] STEP-01 · blocked | a | b | ACCEPTANCE-01 | c |

## Gates

| ID | Gate | Owner | Blocked work | Closure |
|---|---|---|---|---|
| $1 | decision — retry backoff | engineer | STEP-01 | engineer selects strategy |

## Acceptance

| Criterion | Status | Evidence |
|---|---|---|
| ACCEPTANCE-01 | pending | — |
EOF
}

gated '- [ ] GATE-01 · open';        expect_ok  "open gate is unchecked and unstruck" "$TMP/task-plan.md"
gated '- [x] ~~GATE-01~~ · settled'; expect_ok  "settled gate is checked and struck" "$TMP/task-plan.md"
gated '- [x] GATE-01 · settled';     expect_bad "settled gate without strikethrough" "$TMP/task-plan.md"
gated '- [ ] ~~GATE-01~~ · settled'; expect_bad "settled gate left unchecked" "$TMP/task-plan.md"
gated '- [ ] ~~GATE-01~~ · open';    expect_bad "open gate struck early" "$TMP/task-plan.md"
gated '- [x] GATE-01 · completed';   expect_bad "work status used for a gate" "$TMP/task-plan.md"
gated '- [x] ~~GATE-01 · settled';   expect_bad "unbalanced strikethrough markers" "$TMP/task-plan.md"

# Discriminating fence fixture: an unmasked scanner finds the fenced decoy table first
# (wrong columns, unmapped acceptance); a masked scanner sees only the real table.
cat > "$TMP/task-plan.md" <<'EOF'
# Fence fixture

- Profile: Task
- Status: ready
- Cadence: interactive
- Spec context:
  - @specs/x.md
- Next boundary: STEP-01

## Contents

- [Steps](#steps)
- [Acceptance](#acceptance)

## Steps

| ID | Checkable result | Affected surfaces | Acceptance | Verification |
|---|---|---|---|---|
| - [ ] STEP-01 · ready | a | b | ACCEPTANCE-01 | c |

## Acceptance

```text
| wrong | cols |
|---|---|
## Phantom
| - [ ] STEP-01 · ready | decoy |
```

| Criterion | Status | Evidence |
|---|---|---|
| ACCEPTANCE-01 | pending | — |
EOF
expect_ok "fenced content is opaque to section/table scanning" "$TMP/task-plan.md"

# ----------------------------------------------------------------------------
echo "validator tests: $pass passed, $fail failed"
[ "$fail" -eq 0 ]
