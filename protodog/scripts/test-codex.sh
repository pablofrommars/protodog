#!/usr/bin/env bash
# Local integration fixtures only: no model calls, installation, or source-repository commits.
set -euo pipefail
HERE="$(cd "$(dirname "$0")" && pwd)"
python3 "$HERE/../hooks/test-codex-hooks.py"
python3 "$HERE/test-claude-audit.py"
python3 "$HERE/test-codex-package.py"
