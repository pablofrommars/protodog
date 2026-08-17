---
name: program
description: Start or resume the Protodog Program profile for a long-lived multi-track objective. Use when the engineer invokes the Program entry with intent, input/spec references, or an existing program plan.
---

# Program profile entry

You are executing Protodog's Program profile. The protocol package root is
`${CLAUDE_PLUGIN_ROOT}` (in the protocol home repository without the plugin installed, it is
`protodog/`). Load and follow, in this order: `${CLAUDE_PLUGIN_ROOT}/core/foundation.md`,
`${CLAUDE_PLUGIN_ROOT}/core/program-protocol.md`, and apply
`${CLAUDE_PLUGIN_ROOT}/core/verification-and-assurance.md` and
`${CLAUDE_PLUGIN_ROOT}/core/git-policy.md` as referenced. The core documents are authority; this
skill only carries invocation mechanics.

## Arguments

Identical contract to the Task entry: intent and/or exact references or an existing program plan,
plus plain-language overrides only when they differ from defaults (`scope:`, `plan:`,
`cadence: continuous`, `authority:`, `assurance:`). Omitted overrides take defaults silently; the
engineer never authors XML or protocol structure.

## Procedure

1. Record Program as the engineer-selected profile.
2. Resolve the program plan in the current repository exactly as the Task entry resolves a task
   plan, creating from `${CLAUDE_PLUGIN_ROOT}/templates/program-plan.md` when none matches.
3. Assume the engineer-supplied Program worktree; inspect and reconcile state without an entry
   approval stop.
4. Ground the Program; create or refine spec context; run the assurance interrogation once for
   Program-wide audit and paid-evaluation scheduling as planning opens; discover the known track
   topology, dependencies, and acceptance ownership without pre-creating empty track plans.
5. Create or resume only the selected next track plans from
   `${CLAUDE_PLUGIN_ROOT}/templates/track-plan.md`, at execution-ready detail, each opening its
   planning with the assurance interrogation for its assigned scope. Program and track execution
   readiness are declared boundaries: report planning complete with the Foundation's model
   handoff before the first step of that scope executes.
6. Orchestrate under the Program Protocol: you own the program plan, the Program worktree, and
   engineer-facing HIL at all times; dispatch through native subagents (isolated worktrees for any
   concurrent writer); reconcile serially under the Git policy; stop only for Foundation gates,
   declared boundaries, required engineer actions, or declared Program completion.
