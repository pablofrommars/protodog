# Host runtime

This is the mechanics adapter for the shared Task protocol. It changes no plan format,
acceptance rule, cadence, or authority. Load it at Task entry and resumption.

## Package and repository roots

The package root is `../..` relative to the directory containing `skills/task/SKILL.md`. Resolve it to an
absolute path; use that path for core documents, templates, validators, and scripts. It is not
the repository being worked on. Run repository checks and the audit launcher from the target
worktree root. In plugin hooks, `PLUGIN_ROOT` (Codex) and `CLAUDE_PLUGIN_ROOT` (Claude) identify
the installed package; do not assume either variable exists in ordinary shell tool calls.

## Host constraints

Both hosts run shell commands under a sandbox the repository configures, and a refused command
arrives as an ordinary error — connection refused, permission denied, command not found — not
as a permission prompt. Grounding reads the configuration and states its facts: on Claude Code the
`sandbox` block of `.claude/settings.json` (write roots under `filesystem.allowWrite`, the network
allowlist, denied read paths such as `~/.ssh` and `~/.config/gh`), on Codex `sandbox_mode` and its
network access, where `workspace-write` means no network and no writes outside the workspace and
`/tmp`. Consequences that recur: writer worktrees are created inside the repository's writable
roots (`.claude/worktrees/` or a repository-local `.worktrees/`), never as a sibling directory;
remote Git operations and package restores need network the sandbox may not grant, and the hooks
own the remote operations anyway; `gh` and SSH remotes read denied paths; nothing writes to the
home directory. A refused command is handled under the assurance policy's retry rule — change it,
move it inside the write roots, or raise the permission once — and is distinguished from a hook
denial, which names its reason and is never retried. macOS ships BSD tools: no `timeout`, `sed -i`
takes an argument, `grep -P` is absent; prefer portable forms or the language runtime.

The Foundation's routing ladder has three rungs — standard, advanced, top — and the small tier
below it, never used for a Task. Each host section names the rungs' current families; model
identities stay in the agent-type definitions.

## Claude Code

Entry and resumption use `/protodog:task`. Challenge routing uses `agents/challenge.md`. The external auditor is
Codex: `dotnet <package-root>/scripts/audit-launch.cs <dispatch-path>`.

The rungs are the current Sonnet, Opus, and Fable families; Haiku is the small tier, usable only
for read-only survey subagents. The Agent tool's model parameter selects a rung per subagent; a
fork inherits the session model and cannot be downshifted. `/protodog:coordinate` dispatches a
runbook's units from one session as isolated-worktree subagents under this adapter; it is not
exposed on Codex. The ideation mini protocols (`/protodog:ideate-feature`, `-concept`, `-domain`),
`/protodog:runbook`, `/protodog:persist`, `/protodog:interrogate`, and `/protodog:commit-message`
take the invocation text as their argument and may inject repository state through shell before
the prompt is read.

## Codex

Entry and resumption use the installed `task` skill, selected explicitly from `/skills` or with `$protodog:task`; for example:

```text
$protodog:task plan: @plan/retry-fix/task-plan.md
```

1. Before creating or changing Protocol state, verify that the installed plugin's tool hooks
   are enabled and trusted. `/hooks` is the engineer's inspection and trust interface. A
   disabled, untrusted, or unavailable hook is a setup blocker, not permission to substitute
   model-only validation. Use the installation smoke check in the README when setting up a host.
2. Follow the shared Task entry procedure. Read `agents/challenge-codex.toml` for the planning
   and challenge model configuration; it also names the routing ladder's rungs on this host. Model availability is account-specific; if that
   configuration is unavailable, surface a model-selection gate instead of silently choosing
   another. Readiness pauses in both cadences. A fresh session resuming a ready plan still
   needs the engineer's release of its next boundary; an invocation explicitly requesting
   execution or `go` supplies it. Do not replay completed planning or duplicate the plan.
3. Keep the coordinating agent as the only repository writer. Bounded read-only subagent
   assignments are permitted when useful; explicitly request fresh context and give only the
   scoped brief when independence matters. Codex subagents inherit permissions and can share
   the working directory: read-only instructions are a behavioral restriction, not a sandbox.
   Concurrent writers are outside this adapter's supported scope.
4. Challenge and remediation execute on the pinned top configuration. If the coordinating
   session is below it, present the exact model/effort and `$protodog:task plan: @...` handoff and wait
   for a fresh coordinating session. Do not delegate writes into the shared worktree to a
   challenge subagent. The TOML file is routing data; plugin installation does not register it
   as a native custom agent.
5. The cross-vendor auditor is Claude. After the normal per-audit authorization gate, run:
   `dotnet <package-root>/scripts/audit-launch.cs --auditor claude <dispatch-path>`.
   Inspect `--dry-run --auditor claude <dispatch-path>` while preparing the gate. Disclose that
   this backend has only Read/Glob/Grep, so it cannot execute tests or shell-based inspections.
   Its reviewed model selector is an alias; the report stamps the resolved model when Claude
   supplies it. No automatic fallback to Codex is allowed. If Claude is unavailable, present
   the manual audit option or a decision on the vendor-diversity limitation.
6. Use a structured question tool only when the current surface exposes an applicable one;
   otherwise ask the same decision-ready question in the conversation. Respect its batch
   limit. Neither an unanswered asynchronous question nor elapsed time releases a gate.
7. After a post-tool rejection, inspect the persisted files and repair or supersede invalid
   state before dependent work. A post-tool hook reports a completed mutation; it does not
   roll it back. Keep canonical mutations synchronous: wait for shell commands to finish
   before subsequent plan/spec writes or dependent execution. Use complete shell commands,
   not interactive shells fed later through `write_stdin`, for canonical or Git mutations.
8. An explicit `no commits` instruction overrides checkpoint delegation. Record this execution
   constraint in the owning plan's Material notes so a fresh session retains it. Preserve
   verified work and current plan state without staging or committing, and identify evidence
   by HEAD plus hashes of the relevant changed/untracked files. This also applies after resumption.

## Shared maintenance

`plan-sweep`, `deferred-review`, and `working-canvas` use the same artifact contracts in both
hosts. Resolve their package root relative to their loaded skill directory as above. In Codex use
`$protodog:plan-sweep`, `$protodog:deferred-review`, and `$protodog:working-canvas`. The ideation
mini protocols, `runbook`, `persist`, `interrogate`, and `commit-message` are exposed in both
hosts; on Codex (`$protodog:ideate-feature …`, `$protodog:runbook …`) the text after the skill
mention is the argument, nothing is substituted, and the package build rewrites each shell
injection into an instruction to inspect the same fact by hand.
Deferred review supports `triage` and `verify <handoff-path>` in both hosts; the engineer selects
the model before each invocation. The handoff is portable evidence under `inputs/`, not a model
launcher or an independent audit. Maintenance never implies a grant to commit when the engineer
has withheld it.
