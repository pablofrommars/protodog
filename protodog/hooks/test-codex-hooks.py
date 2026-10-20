#!/usr/bin/env python3
"""Exercise actual hook decisions with Codex wire payloads and disposable repositories."""
import json
import os
from pathlib import Path
import subprocess
import tempfile
import unittest

PACKAGE = Path(__file__).resolve().parents[1]


class CodexHooks(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(prefix="protodog-codex-hooks-")
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name).resolve()
        self.env = {k: v for k, v in os.environ.items()
                    if k not in ("CLAUDE_PROJECT_DIR", "CLAUDE_PLUGIN_ROOT", "CLAUDE_PLUGIN_DATA")}
        self.env.update(PLUGIN_ROOT=str(PACKAGE), PROTOCOL_DENIAL_LOG=str(self.root / "denials.log"))
        subprocess.run(["git", "init", "-q", str(self.root)], check=True)
        (self.root / "specs").mkdir()
        (self.root / "plan/p/audits").mkdir(parents=True)
        (self.root / "specs/bound.md").write_text(
            "# Bound spec\n\n## Acceptance criteria\n\n"
            "| Criterion | Condition |\n|---|---|\n| ACCEPTANCE-01 | Works. |\n")
        self.plan = self.root / "plan/p/task-plan.md"
        self.plan.write_text((PACKAGE / "templates/task-plan.md").read_text()
                             .replace("@specs/<document>.md", "@specs/bound.md")
                             .replace("Status: pending planning", "Status: ready"))
        (self.root / "plan/p/audits/audit-01.md").write_text("# Existing report\n")

    def hook(self, mode, command, *, tool="apply_patch", call="call-1", cwd=None, expected=0):
        payload = dict(session_id="codex-test", tool_use_id=call, tool_name=tool,
                       cwd=str(cwd or self.root), tool_input=dict(command=command))
        result = subprocess.run(["dotnet", str(PACKAGE / "hooks/protodog-hooks.cs"), mode],
                                input=json.dumps(payload), text=True, capture_output=True,
                                cwd=self.root, env=self.env)
        self.assertEqual(result.returncode, expected, result.stdout + result.stderr)
        return result

    def patch(self, body):
        return "*** Begin Patch\n" + body + "\n*** End Patch"

    def test_bound_spec_all_patch_operations(self):
        for body in ("*** Update File: specs/bound.md\n@@\n-x\n+y",
                     "*** Delete File: specs/bound.md",
                     "*** Update File: specs/bound.md\n*** Move to: specs/new.md\n@@\n-x\n+y",
                     "*** Update File: other.md\n*** Move to: specs/bound.md\n@@\n-x\n+y",
                     "*** Add File: specs/bound.md\n+replacement"):
            with self.subTest(body=body):
                self.hook("pre-write", self.patch(body), expected=2)

    def test_later_file_in_batch_is_checked(self):
        self.hook("pre-write", self.patch("*** Add File: harmless.txt\n+hello\n"
                                         "*** Delete File: plan/p/audits/audit-01.md"), expected=2)

    def test_unbound_spec_and_new_report(self):
        self.hook("pre-write", self.patch("*** Add File: specs/new.md\n+new\n"
                                         "*** Add File: plan/p/audits/audit-02.md\n+report"))

    def test_cwd_subdirectory_and_symlinks(self):
        sub = self.root / "nested"
        sub.mkdir()
        self.hook("pre-write", self.patch("*** Delete File: ../specs/bound.md"), cwd=sub, expected=2)
        (self.root / "alias").symlink_to(self.root / "specs", target_is_directory=True)
        self.hook("pre-write", self.patch("*** Delete File: alias/bound.md"), expected=2)

    def test_patch_plan_removal_and_relocation(self):
        for body in ("*** Delete File: plan/p/task-plan.md",
                     "*** Update File: plan/p/task-plan.md\n*** Move to: archived.md\n@@\n-x\n+y"):
            self.hook("pre-write", self.patch(body), expected=2)

    def test_post_patch_validates_every_plan(self):
        patch = self.patch("*** Add File: ordinary.txt\n+ok\n*** Update File: plan/p/task-plan.md\n@@\n-x\n+y")
        self.hook("post-plan-write", patch)
        self.plan.write_text(self.plan.read_text().replace("Status: ready", "Status: imaginary"))
        self.hook("post-plan-write", patch, expected=2)

    def test_invalid_hook_input_fails_closed(self):
        self.hook("pre-write", "not a patch", expected=2)
        self.hook("pre-write", self.patch("*** Move to: other.md"), expected=2)
        for payload in ("{broken", "{}", '{"tool_input":{"file_path":12}}'):
            result = subprocess.run(["dotnet", str(PACKAGE / "hooks/protodog-hooks.cs"), "pre-write"],
                                    input=payload, text=True, capture_output=True,
                                    cwd=self.root, env=self.env)
            self.assertEqual(result.returncode, 2, result.stderr)

    def test_shell_fence(self):
        self.hook("pre-bash", "git push origin main", tool="Bash", expected=2)
        self.hook("pre-bash", "git status", tool="Bash")
        self.hook("post-bash", "git status", tool="Bash")

    def test_identical_overlapping_commands_have_distinct_snapshots(self):
        command = "some-long-running-command"
        self.hook("pre-bash", command, tool="Bash", call="first")
        self.hook("pre-bash", command, tool="Bash", call="second")
        self.hook("post-bash", command, tool="Bash", call="first")
        (self.root / "specs/bound.md").write_text("changed while second command ran")
        self.hook("post-bash", command, tool="Bash", call="second", expected=2)

    def test_task_lifecycle_and_persisted_position(self):
        patch = self.patch("*** Update File: plan/p/task-plan.md\n@@\n-x\n+y")
        self.hook("post-plan-write", patch)
        self.plan.write_text(self.plan.read_text().replace("Status: ready", "Status: in progress")
                             .replace("⬜ STEP-01 · ready", "🟡 STEP-01 · in progress"))
        self.hook("post-plan-write", patch)
        # A newly resumed process validates the durable artifact and computes position.
        position = subprocess.run(["dotnet", str(PACKAGE / "validators/validate-plan.cs"),
                                   "--position", str(self.plan)], text=True, capture_output=True,
                                  cwd=self.root, env=self.env)
        self.assertEqual(position.returncode, 0, position.stderr)
        self.assertIn("active STEP-01", position.stdout)
        self.plan.write_text(self.plan.read_text().replace("Status: in progress", "Status: completed")
                             .replace("🟡 STEP-01 · in progress", "✅ STEP-01 · completed")
                             .replace("- Next boundary: STEP-01\n", ""))
        self.hook("post-plan-write", patch, expected=2)  # Unsettled acceptance prevents completion.
        self.plan.write_text(self.plan.read_text().replace("⬜ ACCEPTANCE-01 · pending | —",
                                                          "✅ ACCEPTANCE-01 · satisfied | verified"))
        self.hook("post-plan-write", patch)


if __name__ == "__main__":
    unittest.main()
