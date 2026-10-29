#!/usr/bin/env python3
"""Build and exercise a relocated package without touching user Codex configuration."""
import json
import os
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest

PACKAGE = Path(__file__).resolve().parents[1]


class CodexPackage(unittest.TestCase):
    def test_relocated_package(self):
        with tempfile.TemporaryDirectory(prefix="protodog-package-") as directory:
            root = Path(directory).resolve()
            built = root / "marketplace"
            builder = PACKAGE / "scripts/build-codex-package.cs"
            result = subprocess.run(["dotnet", str(builder), str(built)], capture_output=True, text=True)
            self.assertEqual(result.returncode, 0, result.stdout + result.stderr)
            again = subprocess.run(["dotnet", str(builder), str(built)], capture_output=True, text=True)
            self.assertEqual(again.returncode, 2, again.stdout + again.stderr)
            installed = root / "installed elsewhere"
            shutil.move(str(built), installed)
            catalog = json.loads((installed / ".agents/plugins/marketplace.json").read_text())
            package = installed / catalog["plugins"][0]["source"]["path"]
            manifest = json.loads((package / "plugin.json").read_text())
            self.assertEqual(manifest["name"], catalog["plugins"][0]["name"])
            hook_path = manifest["extensions"]["com.openai"]["hooks"]
            self.assertEqual(hook_path, "./hooks/hooks.json")
            self.assertTrue((package / hook_path).is_file())
            skills = {p.name for p in (package / "skills").iterdir()}
            self.assertEqual(skills, {"task", "plan-sweep", "deferred-review", "working-canvas", "ideate-feature", "ideate-concept", "ideate-domain", "runbook", "persist", "interrogate", "commit-message"})
            self.assertFalse((package / ".claude-plugin").exists())
            self.assertTrue((package / "agents/challenge-codex.toml").is_file())
            self.assertFalse((package / "agents/challenge.md").exists())
            for filename in ("core/task-protocol.md", "core/host-runtime.md", "validators/validate-plan.cs", "scripts/sweep-check.cs", "scripts/worktree-survey.cs"):
                self.assertEqual((package / filename).read_bytes(), (PACKAGE / filename).read_bytes())
            # Maintenance handoffs must survive packaging, including mode-specific references.
            shared_skill = PACKAGE / "skills/deferred-review"
            installed_skill = package / "skills/deferred-review"
            source_files = {p.relative_to(shared_skill) for p in shared_skill.rglob("*") if p.is_file()}
            installed_files = {p.relative_to(installed_skill) for p in installed_skill.rglob("*") if p.is_file()}
            self.assertEqual(installed_files, source_files)
            for filename in source_files:
                self.assertEqual((installed_skill / filename).read_bytes(), (shared_skill / filename).read_bytes())

            target = root / "target"
            target.mkdir()
            env = {k: v for k, v in os.environ.items()
                   if k not in ("CLAUDE_PROJECT_DIR", "CLAUDE_PLUGIN_DATA")}
            env.update(PLUGIN_ROOT=str(package), CLAUDE_PLUGIN_ROOT=str(package),
                       PROTOCOL_DENIAL_LOG=str(root / "denials.log"))
            hooks = json.loads((package / "hooks/hooks.json").read_text())["hooks"]
            command = hooks["PreToolUse"][0]["hooks"][0]["command"]
            payload = json.dumps(dict(cwd=str(target), session_id="relocated", tool_use_id="one",
                                      tool_name="Bash", tool_input=dict(command="git push")))
            check = subprocess.run(command, shell=True, input=payload, text=True,
                                   capture_output=True, cwd=target, env=env)
            self.assertEqual(check.returncode, 2, check.stdout + check.stderr)
            self.assertIn("publishing refs is engineer-owned", check.stderr)

            validator = subprocess.run(["dotnet", str(package / "validators/validate-plan.cs"),
                                        str(package / "templates/task-plan.md")],
                                       cwd=target, capture_output=True, text=True)
            self.assertEqual(validator.returncode, 0, validator.stdout + validator.stderr)


if __name__ == "__main__":
    unittest.main()
