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
expect_ok "template deferred register" "$T/deferred-register.md"

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
seed; awk '{print} /^\| ⬜ STEP-01/{print}' "$TMP/task-plan.md" > "$TMP/dup.md"; mv "$TMP/dup.md" "$TMP/task-plan.md"
expect_bad "duplicate step id" "$TMP/task-plan.md"

seed; sed -i '' 's/| ACCEPTANCE-01 | <command/| ACCEPTANCE-99 | <command/' "$TMP/task-plan.md"
expect_bad "unmapped acceptance reference" "$TMP/task-plan.md"

seed; printf '\n## Gates\n' >> "$TMP/task-plan.md"; sed -i '' 's|^- \[Acceptance\](#acceptance)$|&\
- [Gates](#gates)|' "$TMP/task-plan.md"
expect_bad "empty optional section" "$TMP/task-plan.md"

seed; sed -i '' 's/- Next boundary: STEP-01/- Next boundary: STEP-07/' "$TMP/task-plan.md"
expect_bad "next boundary references unknown step" "$TMP/task-plan.md"

# --- id cell: marker + id + status, marker and label must agree --------------
seed; sed -i '' 's/⬜ STEP-01 · ready/🟡 STEP-01 · in progress/' "$TMP/task-plan.md"
expect_ok "active marker on an in-progress step" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ STEP-01 · ready/⬛ STEP-01 · cancelled/' "$TMP/task-plan.md"
expect_ok "closed-not-met marker on a cancelled step" "$TMP/task-plan.md"

seed; sed -i '' 's/| ⬜ STEP-01 · ready |/| STEP-01 |/' "$TMP/task-plan.md"
expect_bad "bare id without marker and status" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ STEP-01 · ready/⬜ STEP-01/' "$TMP/task-plan.md"
expect_bad "marker without status label" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ STEP-01 · ready/- [ ] STEP-01 · ready/' "$TMP/task-plan.md"
expect_bad "retired checkbox grammar" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ STEP-01 · ready/✅ STEP-01 · ready/' "$TMP/task-plan.md"
expect_bad "done marker on a ready step" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ STEP-01 · ready/⬜ STEP-01 · completed/' "$TMP/task-plan.md"
expect_bad "not-started marker on a completed step" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ STEP-01 · ready/🟡 STEP-01 · blocked/' "$TMP/task-plan.md"
expect_bad "active marker on a blocked step" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ STEP-01 · ready/❌ STEP-01 · blocked/' "$TMP/task-plan.md"
expect_bad "marker outside the defined set" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ STEP-01 · ready/⬜ STEP-01 · doing stuff/' "$TMP/task-plan.md"
expect_bad "status outside enum inside id cell" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ STEP-01 · ready/⬜ ~~STEP-01~~ · ready/' "$TMP/task-plan.md"
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

# --- acceptance: same markers, own enum -------------------------------------
# 🔴 for unmet is the point of the marker set: a criterion checked and failed is visibly
# distinct from one merely pending, which a binary checkbox could not express.
seed; sed -i '' 's/⬜ ACCEPTANCE-01 · pending/✅ ACCEPTANCE-01 · satisfied/' "$TMP/task-plan.md"
expect_ok "satisfied criterion is done" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ ACCEPTANCE-01 · pending/⬛ ACCEPTANCE-01 · accepted exception/' "$TMP/task-plan.md"
expect_ok "accepted exception is closed without being met" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ ACCEPTANCE-01 · pending/🔴 ACCEPTANCE-01 · unmet/' "$TMP/task-plan.md"
expect_ok "unmet criterion needs attention" "$TMP/task-plan.md"

seed; sed -i '' 's/| ⬜ ACCEPTANCE-01 · pending |/| ACCEPTANCE-01 |/' "$TMP/task-plan.md"
expect_bad "bare acceptance criterion id" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ ACCEPTANCE-01 · pending/✅ ACCEPTANCE-01 · pending/' "$TMP/task-plan.md"
expect_bad "done marker on a pending criterion" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ ACCEPTANCE-01 · pending/⬜ ACCEPTANCE-01 · satisfied/' "$TMP/task-plan.md"
expect_bad "not-started marker on a satisfied criterion" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ ACCEPTANCE-01 · pending/⬜ ACCEPTANCE-01 · unmet/' "$TMP/task-plan.md"
expect_bad "unmet criterion not marked for attention" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ ACCEPTANCE-01 · pending/⬜ ACCEPTANCE-01 · completed/' "$TMP/task-plan.md"
expect_bad "work status used for a criterion" "$TMP/task-plan.md"

seed; sed -i '' 's/⬜ ACCEPTANCE-01 · pending/✅ ~~ACCEPTANCE-01~~ · satisfied/' "$TMP/task-plan.md"
expect_bad "strikethrough on an acceptance id" "$TMP/task-plan.md"

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
| 🔴 STEP-01 · blocked | a | b | ACCEPTANCE-01 | c |

## Gates

| ID | Gate | Owner | Blocked work | Closure |
|---|---|---|---|---|
| $1 | decision — retry backoff | engineer | STEP-01 | engineer selects strategy |

## Acceptance

| Criterion | Evidence |
|---|---|
| ⬜ ACCEPTANCE-01 · pending | — |
EOF
}

gated '🔴 GATE-01 · open';        expect_ok  "open gate needs attention and is unstruck" "$TMP/task-plan.md"
gated '✅ ~~GATE-01~~ · settled'; expect_ok  "settled gate is done and struck" "$TMP/task-plan.md"
gated '✅ GATE-01 · settled';     expect_bad "settled gate without strikethrough" "$TMP/task-plan.md"
gated '🔴 ~~GATE-01~~ · settled'; expect_bad "settled gate carrying the open marker" "$TMP/task-plan.md"
gated '🔴 ~~GATE-01~~ · open';    expect_bad "open gate struck early" "$TMP/task-plan.md"
gated '⬜ GATE-01 · open';        expect_bad "wrong marker on an open gate" "$TMP/task-plan.md"
gated '✅ GATE-01 · completed';   expect_bad "work status used for a gate" "$TMP/task-plan.md"
gated '✅ ~~GATE-01 · settled';   expect_bad "unbalanced strikethrough markers" "$TMP/task-plan.md"

# --- terminal lift: a terminal plan is not the sole carrier of a live obligation ---
lifted() { # status, deferred item line -> a task plan carrying both
  cat > "$TMP/task-plan.md" <<EOF
# Lift fixture

- Profile: Task
- Status: $1
- Cadence: interactive
- Spec context:
  - @specs/x.md

## Contents

- [Steps](#steps)
- [Acceptance](#acceptance)
- [Deferred issues and accepted gaps](#deferred-issues-and-accepted-gaps)

## Steps

| ID | Checkable result | Affected surfaces | Acceptance | Verification |
|---|---|---|---|---|
| ✅ STEP-01 · completed | a | b | ACCEPTANCE-01 | c |

## Acceptance

| Criterion | Evidence |
|---|---|
| ✅ ACCEPTANCE-01 · satisfied | done |

## Deferred issues and accepted gaps

- $2
EOF
}

lifted 'completed' 'retry backoff tuning → D-03'
expect_ok "terminal plan with lifted deferred item" "$TMP/task-plan.md"

lifted 'completed' 'retry backoff tuning → closed: obsoleted by STEP-01'
expect_ok "terminal plan with closed deferred item" "$TMP/task-plan.md"

lifted 'completed' 'retry backoff tuning'
expect_bad "terminal plan with un-lifted deferred item" "$TMP/task-plan.md"

lifted 'cancelled' 'retry backoff tuning'
expect_bad "cancelled plan with un-lifted deferred item" "$TMP/task-plan.md"

lifted 'in progress' 'retry backoff tuning'
expect_ok "live plan carries deferred item without arrow" "$TMP/task-plan.md"

# A wrapped item may carry its arrow on the final line — the item closes with it, not the lead line.
lifted 'completed' 'retry backoff tuning, with a rationale long enough that the bullet wraps
  onto a continuation line → D-59'
expect_ok "terminal plan with wrapped item, arrow on final line" "$TMP/task-plan.md"

lifted 'completed' 'retry backoff tuning, with a rationale long enough that the bullet wraps
  onto a continuation line and never records a disposition'
expect_bad "terminal plan with wrapped un-lifted item" "$TMP/task-plan.md"

# --- deferred register --------------------------------------------------------
reg() { # parked id cell, disposition -> a register carrying one parked and one closed row
  cat > "$TMP/deferred.md" <<EOF
# Deferred register

## Contents

- [Parked](#parked)
- [Closed](#closed)

## Parked

| ID | Item | Disposition | Revisit | Provenance |
|---|---|---|---|---|
| $1 | claim, why it matters, what settles it | $2 | — | plan-x DEFERRED-01, 2026-08-13 |

## Closed

| ID | Outcome |
|---|---|
| ✅ D-09 · closed | done by task-y, 2026-08-13 |
EOF
}

reg '⬜ D-01 · parked' 'actionable';       expect_ok  "register parked row" "$TMP/deferred.md"
reg '⬜ D-01 · parked' "engineer's call";  expect_ok  "register disposition with apostrophe" "$TMP/deferred.md"
reg '⬜ D-01 · parked' 'someday';          expect_bad "register disposition outside enum" "$TMP/deferred.md"
reg '✅ D-01 · closed' 'actionable';       expect_bad "closed status under Parked" "$TMP/deferred.md"
reg '⬛ D-01 · dropped' 'actionable';      expect_bad "dropped status under Parked" "$TMP/deferred.md"
reg '⬜ D-09 · parked' 'actionable';       expect_bad "duplicate register id across sections" "$TMP/deferred.md"
reg '⬜ D-01 · pending' 'actionable';      expect_bad "register status outside enum" "$TMP/deferred.md"

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
| ⬜ STEP-01 · ready | a | b | ACCEPTANCE-01 | c |

## Acceptance

```text
| wrong | cols |
|---|---|
## Phantom
| ⬜ STEP-01 · ready | decoy |
```

| Criterion | Evidence |
|---|---|
| ⬜ ACCEPTANCE-01 · pending | — |
EOF
expect_ok "fenced content is opaque to section/table scanning" "$TMP/task-plan.md"

# ----------------------------------------------------------------------------
echo "validator tests: $pass passed, $fail failed"
[ "$fail" -eq 0 ]
