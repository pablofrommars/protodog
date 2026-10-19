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

# --- live path with a fake auditor on PATH (still no Codex contact) ----------------
mkdir -p "$TMP/bin" "$TMP/log" "$TMP/repo/audits"
cat > "$TMP/bin/codex" <<'EOF'
#!/usr/bin/env bash
out=""; while [ $# -gt 0 ]; do [ "$1" = "--output-last-message" ] && out="$2"; shift; done
[ -n "${FAKE_TOUCH:-}" ] && echo drift >> "$FAKE_TOUCH"
printf '# report\n' > "$out"
exit "${FAKE_EXIT:-0}"
EOF
chmod +x "$TMP/bin/codex"
printf 'target\n' > "$TMP/repo/target.md"
H2="$(shasum -a 256 "$TMP/repo/target.md" | cut -d' ' -f1)"
for n in 05 06 07; do printf '# AUDIT-%s Dispatch\n\n| `%s` | `target.md` |\n' "$n" "$H2" > "$TMP/repo/audits/audit-$n-dispatch.md"; done
live() { ( export PATH="$TMP/bin:$PATH" CLAUDE_PROJECT_DIR="$TMP/repo" PROTOCOL_AUDIT_LOG="$TMP/log/audit-launch.log"; dotnet "$HERE/audit-launch.cs" "$@" ); }

ok live "$TMP/repo/audits/audit-05-dispatch.md"                         # clean exit captures
ok grep -q 'launched by audit-launch.cs' "$TMP/repo/audits/audit-05.md"  # provenance stamp
no grep -q 'DRIFT' "$TMP/repo/audits/audit-05.md"                        # no drift stamped

export FAKE_EXIT=1
no live "$TMP/repo/audits/audit-06-dispatch.md"                         # nonzero exit refuses
unset FAKE_EXIT
no test -e "$TMP/repo/audits/audit-06.md"                                # nothing captured under audits/
ok ls "$TMP"/log/audit-06.rejected.*.md                                  # partial output parked beside the log

export FAKE_TOUCH="$TMP/repo/target.md"
ok live "$TMP/repo/audits/audit-07-dispatch.md"                         # drift is captured, not refused
unset FAKE_TOUCH
ok grep -q 'DRIFT=target.md' "$TMP/repo/audits/audit-07.md"              # and stamped into the report

echo "launcher tests: $pass passed, $fail failed"
[ "$fail" -eq 0 ]
