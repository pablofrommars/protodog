#!/usr/bin/env bash
# test-registry.sh — behavioral tests for validate-registry.js.
set -u
HERE="$(cd "$(dirname "$0")" && pwd)"
ROOT="$(cd "$HERE/../.." && pwd)"
V="dotnet $HERE/validate-registry.cs"
R="$ROOT/protodog/registries"
TMP="$(mktemp -d)"
trap 'rm -rf "$TMP"' EXIT
pass=0; fail=0
ok() { local n="$1"; shift; if $V "$@" >/dev/null 2>&1; then pass=$((pass+1)); else fail=$((fail+1)); echo "FAIL (expected valid): $n"; $V "$@" 2>&1 | sed 's/^/    /'; fi }
no() { local n="$1"; shift; if $V "$@" >/dev/null 2>&1; then fail=$((fail+1)); echo "FAIL (expected rejection): $n"; else pass=$((pass+1)); fi }

ok "successor registries together" "$R/prompt-utility.code-snippets" "$R/boxer.code-snippets"

printf '{ "A": { "scope": "markdown", "prefix": "p-x", "body": ["${1:a} ${2:b}"], "description": "d" } }' > "$TMP/ok.code-snippets"
ok "minimal valid registry" "$TMP/ok.code-snippets"

printf '{ "A": { "scope": "markdown", "prefix": "p-protocol-x", "body": ["x"], "description": "d" } }' > "$TMP/r.code-snippets"
no "family prefix without declared kind" "$TMP/r.code-snippets"

printf '{ "A": { "scope": "markdown", "prefix": "p-x", "body": ["x"], "description": "a mini protocol" } }' > "$TMP/r2.code-snippets"
no "mini-protocol description without family prefix" "$TMP/r2.code-snippets"

printf '{ "A": { "scope": "markdown", "prefix": "p-protocol-x", "body": ["x"], "description": "a mini protocol" } }' > "$TMP/r3.code-snippets"
ok "coherent mini-protocol family entry" "$TMP/r3.code-snippets"

printf '{ "A": { "scope": "markdown", "prefix": "p-protocol-x", "body": ["x"], "description": "a protocol utility" } }' > "$TMP/r4.code-snippets"
ok "coherent protocol-utility family entry" "$TMP/r4.code-snippets"

printf '{ "A": { "scope": "markdown", "prefix": "p-x", "body": ["x"], "description": "a protocol utility" } }' > "$TMP/r5.code-snippets"
no "protocol-utility description without family prefix" "$TMP/r5.code-snippets"

printf '{ "A": { "scope": "markdown", "prefix": "p-x", "body": ["x"], "description": "d" }, "B": { "scope": "markdown", "prefix": "p-x", "body": ["y"], "description": "d" } }' > "$TMP/d.code-snippets"
no "duplicate prefix" "$TMP/d.code-snippets"

printf '{ "A": { "scope": "markdown", "prefix": "p-x", "body": ["${1:a} ${3:c}"], "description": "d" } }' > "$TMP/g.code-snippets"
no "tab-stop gap" "$TMP/g.code-snippets"

printf '{ "A": { "scope": "markdown", "prefix": "p-x", "body": ["x"] } }' > "$TMP/m.code-snippets"
no "missing description" "$TMP/m.code-snippets"

printf '{ broken json' > "$TMP/b.code-snippets"
no "unparseable file" "$TMP/b.code-snippets"

echo "registry tests: $pass passed, $fail failed"
[ "$fail" -eq 0 ]
