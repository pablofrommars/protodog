---
name: task
description: Start or resume the Protodog Task profile for a bounded engineering objective. Use when the engineer invokes the Task entry with intent, input/spec references, or an existing task plan.
---

# Task profile entry

You are executing Protodog's Task profile. The protocol package root is
`${CLAUDE_PLUGIN_ROOT}` (in the protocol home repository without the plugin installed, it is
`protodog/`). Load and follow, in this order: `${CLAUDE_PLUGIN_ROOT}/core/foundation.md`,
`${CLAUDE_PLUGIN_ROOT}/core/task-protocol.md`, and apply
`${CLAUDE_PLUGIN_ROOT}/core/verification-and-assurance.md` and
`${CLAUDE_PLUGIN_ROOT}/core/git-policy.md` as referenced. The core documents are authority; this
skill only carries invocation mechanics.

## Arguments

The invocation text supplies current-session intent and/or exact `@inputs/` or `@specs/`
references, or an exact existing task-plan reference. Overrides are stated in plain language inside
the invocation and only when they differ from defaults:

- `scope: <narrower scope>`
- `plan: @plan/<plan-id>/task-plan.md`
- `cadence: continuous`
- `authority: <exact additional grant>`
- `assurance: <selected review or audit and its placement>`

Omitted overrides take defaults silently: whole supplied context as grounding input, conventional
plan location, `interactive` cadence, ordinary in-scope authority, audit and paid-evaluation
scheduling settled by the assurance interrogation at planning entry (an omitted `assurance:`
selects nothing by itself). Never ask
about an omitted override; never require the engineer to author XML or protocol structure.

## Procedure

1. Record Task as the engineer-selected profile.
2. Resolve the plan in the current repository: an explicit `plan:` reference resumes it; otherwise
   exactly one matching plan under `plan/` resumes, none creates from
   `${CLAUDE_PLUGIN_ROOT}/templates/task-plan.md` (plan id = kebab-case slug of the Task label,
   unique under `plan/`), multiple plausible matches become a Foundation HIL request.
3. Assume the engineer-supplied worktree; inspect and reconcile repository state and policy without
   an entry approval stop.
4. Ground the supplied context; create or refine the smallest sufficient spec context; plan the
   known path end to end, opening planning with the assurance interrogation (audit and
   paid-evaluation scheduling); bind exact specs only at execution readiness.
5. Execute under the Task Protocol — no plan-approval stop; stop only for Foundation gates,
   declared boundaries, required engineer actions, or completion.
