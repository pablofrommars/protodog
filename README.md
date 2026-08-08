# Protodog

## Why this document exists

Installation and maintenance guide for Protodog, the Claude-native agentic execution protocol,
distributed from this repository as a Claude Code plugin. The normative authority is
`protodog/core/`; this file never overrides it.

## Prerequisites

- .NET SDK ≥ 10 (hooks, validators, and the audit launcher are file-based apps; no packages, no Node)
- bash, git
- Claude Code (skills + hooks host)
- Codex CLI, authenticated — only for launching independent audits

## Install (once per engineer — the plugin follows you across repositories)

This repository is a Claude Code plugin marketplace (`kennel`) serving the `protodog` plugin. In
Claude Code:

```text
/plugin marketplace add <path-or-git-url-of-this-repo>
/plugin install protodog@kennel
```

That single install gives every repository you open the entry skills and the enforcement hooks —
no per-repo copies, no settings merges. Per-repo opt-out: `/plugin disable protodog@kennel` in
that project. (`protodog/hooks/settings-fragment.json` remains only as the wiring fallback for a
repository that vendors `protodog/` directly.)

Per-engineer, editor-level (optional, host-portable prompts): `prompt-utility.code-snippets` is a
personal, repo-agnostic registry — install once at the VS Code user level, symlinked so this repo
stays the single canonical source:

```bash
ln -s "$(pwd)/protodog/registries/prompt-utility.code-snippets" \
  "$HOME/Library/Application Support/Code/User/snippets/prompt-utility.code-snippets"
```

`boxer.code-snippets` is a declared project family: copy it into the `.vscode/` of the BoxerUI
repositories that use it, nowhere else.

## Release ritual

Change → full sweep green → bump `protodog/.claude-plugin/plugin.json` version → `CHANGELOG.md`
entry → commit. Consumers pull with `/plugin marketplace update kennel` and
`/plugin update protodog`. Protocol changes to this repository run under Protodog itself (plans
under `plan/`, specs under `specs/`).

## Verify

```bash
./protodog/validators/test-validators.sh   # plan/spec/dispatch validation
./protodog/validators/test-registry.sh     # snippet registries
./protodog/hooks/test-hooks.sh             # enforcement fences (incl. plugin-mode resolution)
./protodog/scripts/test-audit-launch.sh    # launcher gates (dry-run, no Codex contact)
```

CI runs the same four suites plus manifest checks on every push (macOS runner: the test drivers
use BSD sed).

## Use

- Start bounded work with `/protodog:task`, long-lived multi-track work with `/protodog:program`;
  supply intent or exact `@inputs/` / `@specs/` references. Artifacts land under `inputs/`,
  `specs/`, and `plan/<plan-id>/` of the repository you're in, per `protodog/core/foundation.md`.
- Audits are selected at planning time. When execution reaches the declared placement, the agent
  persists `audit-NN-dispatch.md` and requests launch approval (cost + data egress); then:

  ```bash
  dotnet protodog/scripts/audit-launch.cs plan/<plan-id>/audits/audit-NN-dispatch.md
  ```

  `--dry-run` verifies every gate without contacting Codex. Reports are immutable; a re-launch is
  a new numbered cycle.
- External ideation (Codex or any host): expand `p-ideate-repo` / `p-ideate-protocol` from the
  registry; persist results into `inputs/` with cited provenance.

## Notes

- Warm hook latency is ~190 ms per invocation. If that matters,
  `dotnet publish protodog/hooks/protodog-hooks.cs` produces a native binary — point the hook
  commands at its output.
- Git boundary: the agent commits checkpoints inside its managed worktree; fetch/sync, rebase onto
  `main`, squash, landing, and cleanup remain engineer-owned (`protodog/core/git-policy.md`).
- Hook denials log to the plugin data directory (`CLAUDE_PLUGIN_DATA/protocol-denials.log`;
  repo-local fallback `protodog/logs/hook-denials.log`) — spikes after a model update signal
  protocol-fit regression; a hook that never fires is a deletion candidate.
- Provenance: extracted at v1.0.0 from the prototype workspace
  (`/Users/pablo/source/tmp/protocol`, commit `9ec6704`), which remains the frozen archive of the
  release-candidate lineage, AUDIT-01 cycle, and build history. Design rationale:
  `docs/design-rationale.md`.
