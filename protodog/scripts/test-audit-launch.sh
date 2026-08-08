#!/usr/bin/env bash
# test-audit-launch.sh — dry-run behavioral tests for audit-launch.sh (no Codex contact).
set -u
HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
L="dotnet $HERE/audit-launch.cs"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
cd "$ROOT"
pass=0; fail=0
ok()  { if "$@" >/dev/null 2>&1; then pass=$((pass+1)); else fail=$((fail+1)); echo "FAIL (expected ok): $*"; fi }
no()  { if "$@" >/dev/null 2>&1; then fail=$((fail+1)); echo "FAIL (expected refusal): $*"; else pass=$((pass+1)); fi }

# Synthetic dispatch pinning one real repo file with its true hash.
TARGET="protodog/core/foundation.md"
HASH="$(shasum -a 256 "$ROOT/$TARGET" | cut -d' ' -f1)"
mkdir -p "$TMP/audits"
cat > "$TMP/audits/audit-01-dispatch.md" <<EOF
# AUDIT-01 Dispatch — synthetic test

| SHA-256 | Source |
|---|---|
| \`$HASH\` | \`$TARGET\` |
EOF

ok $L --dry-run "$TMP/audits/audit-01-dispatch.md"                    # valid dispatch passes
no $L --dry-run                                                       # missing argument
no $L --dry-run "$TMP/audits/audit-01-dispatch.md" extra              # extra argument
no $L --dry-run "$TMP/audits/nonexistent-dispatch.md"                 # missing file
cp "$TMP/audits/audit-01-dispatch.md" "$TMP/audits/report.md"
no $L --dry-run "$TMP/audits/report.md"                               # wrong filename shape

sed 's/`[0-9a-f]\{8\}/`deadbeef/' "$TMP/audits/audit-01-dispatch.md" > "$TMP/audits/audit-02-dispatch.md"
no $L --dry-run "$TMP/audits/audit-02-dispatch.md"                    # tampered manifest hash

printf '# AUDIT-03 Dispatch\nno manifest rows here\n' > "$TMP/audits/audit-03-dispatch.md"
no $L --dry-run "$TMP/audits/audit-03-dispatch.md"                    # no verifiable rows

touch "$TMP/audits/audit-01.md"
no $L --dry-run "$TMP/audits/audit-01-dispatch.md"                    # report already exists

echo "launcher tests: $pass passed, $fail failed"
[ "$fail" -eq 0 ]
