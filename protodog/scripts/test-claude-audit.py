#!/usr/bin/env python3
"""Test the Claude backend through a fake CLI, with no model contact."""
import hashlib
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest

PACKAGE = Path(__file__).resolve().parents[1]


class ClaudeAudit(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="protodog-claude-audit-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name).resolve()
        (self.root / "bin").mkdir()
        fake = self.root / "bin/claude"
        fake.write_text("""#!/usr/bin/env python3
import json, os, pathlib, sys
pathlib.Path(os.environ['CAPTURE']).write_text(json.dumps(sys.argv[1:]))
if os.environ.get('DRIFT'):
    pathlib.Path('target.md').write_text('drift')
if os.environ.get('COLLIDE'):
    pathlib.Path('audit-01.md').write_text('existing concurrent report')
print(os.environ.get('RESPONSE', json.dumps({'subtype': 'success', 'is_error': False,
      'result': '# Independent report\\n', 'modelUsage': {'resolved-test-model': {}}})))
sys.exit(int(os.environ.get('FAKE_EXIT', '0')))
""")
        fake.chmod(0o755)
        (self.root / "target.md").write_text("target")
        digest = hashlib.sha256(b"target").hexdigest()
        self.dispatch = self.root / "audit-01-dispatch.md"
        self.dispatch.write_text(f"# Dispatch\n\n| `{digest}` | `target.md` |\n")
        self.report = self.root / "audit-01.md"
        self.env = dict(os.environ, PATH=str(self.root / "bin") + os.pathsep + os.environ["PATH"],
                        CLAUDE_PROJECT_DIR=str(self.root), CAPTURE=str(self.root / "args.json"),
                        PROTOCOL_AUDIT_LOG=str(self.root / "logs/audit.log"))

    def launch(self, *flags, expected=0):
        result = subprocess.run(["dotnet", str(PACKAGE / "scripts/audit-launch.cs"),
                                 *flags, "--auditor", "claude", str(self.dispatch)],
                                cwd=self.root, env=self.env, capture_output=True, text=True)
        self.assertEqual(result.returncode, expected, result.stdout + result.stderr)
        return result

    def test_read_only_tool_surface_and_report(self):
        self.launch()
        args = json.loads((self.root / "args.json").read_text())
        for name, value in (("--tools", "Read,Glob,Grep"), ("--allowedTools", "Read,Glob,Grep"),
                            ("--permission-mode", "dontAsk"), ("--output-format", "json"),
                            ("--model", "fable"), ("--mcp-config", '{"mcpServers":{}}')):
            self.assertEqual(args[args.index(name) + 1], value)
        for flag in ("--safe-mode", "--strict-mcp-config", "--no-session-persistence", "--disable-slash-commands"):
            self.assertIn(flag, args)
        self.assertNotIn("--dangerously-skip-permissions", args)
        report = self.report.read_text()
        self.assertIn("auditor=claude", report)
        self.assertIn("resolved-model=resolved-test-model", report)
        self.assertIn("# Independent report", report)
        self.assertEqual(self.report.stat().st_mode & 0o222, 0)

    def test_dry_run_never_invokes_cli(self):
        self.launch("--dry-run")
        self.assertFalse((self.root / "args.json").exists())
        self.assertFalse(self.report.exists())

    def test_hash_mismatch_never_invokes_cli(self):
        (self.root / "target.md").write_text("changed")
        self.launch(expected=3)
        self.assertFalse((self.root / "args.json").exists())

    def test_failure_and_invalid_results_never_become_reports(self):
        for response in ("not json", '{"is_error":true,"subtype":"error","result":"partial"}',
                         '{"is_error":false,"subtype":"success","result":" "}', '{}'):
            with self.subTest(response=response):
                self.env["RESPONSE"] = response
                self.launch(expected=3)
                self.assertFalse(self.report.exists())
        self.env.pop("RESPONSE")
        self.env["FAKE_EXIT"] = "1"
        self.launch(expected=3)
        self.assertFalse(self.report.exists())
        self.assertTrue(list((self.root / "logs").glob("*.rejected.*.md")))

    def test_drift_is_disclosed(self):
        self.env["DRIFT"] = "1"
        self.launch()
        self.assertIn("DRIFT=target.md", self.report.read_text())

    def test_concurrent_report_is_not_overwritten(self):
        self.env["COLLIDE"] = "1"
        self.launch(expected=3)
        self.assertEqual(self.report.read_text(), "existing concurrent report")

    def test_existing_report_refuses_before_launch(self):
        self.report.write_text("existing")
        self.launch(expected=3)
        self.assertFalse((self.root / "args.json").exists())
        self.assertEqual(self.report.read_text(), "existing")


if __name__ == "__main__":
    unittest.main()
