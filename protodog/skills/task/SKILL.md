---
name: task
description: Start or resume the Protodog Task profile for a bounded engineering objective. Use when the engineer invokes the Task entry with intent, input/spec references, or an existing task plan.
---

# Task profile entry

You are executing Protodog's Task profile. Resolve the package root as `../..` from this
`SKILL.md`'s directory; it is separate from the target repository. Load `core/host-runtime.md` for
the current host's invocation, model routing, and audit mechanics, then `core/foundation.md`,
`core/task-protocol.md`, and apply `core/verification-and-assurance.md` and
`core/git-policy.md` as referenced, all relative to the package root. The core documents are
authority; this skill only carries invocation mechanics.

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
   `<package-root>/templates/task-plan.md` (plan id = kebab-case slug of the Task label,
   unique under `plan/`), multiple plausible matches become a Foundation HIL request. A resumed
   plan is validated first (`dotnet <package-root>/validators/validate-plan.cs
   plan/<plan-id>`) and a rejected artifact is repaired before any other work — a write the hook
   denied may have been left on disk by a session that ended before repairing it.
3. Assume the engineer-supplied worktree; inspect and reconcile repository state and policy without
   an entry approval stop.
4. Ground the supplied context; create or refine the smallest sufficient spec context; plan the
   known path end to end, opening planning with the assurance interrogation (audit and
   paid-evaluation scheduling); verify spec premises against the repository and bind exact specs
   only at execution readiness.
5. Stop at the readiness boundary in every cadence: report planning complete with the
   Foundation's model handoff; the resumption invocation it carries is
   the host's Task invocation with `plan: @plan/<plan-id>/task-plan.md` plus any overrides; `go` proceeds in
   this session.
6. Execute under the Task Protocol — no plan-approval stop; stop only for Foundation gates,
   declared boundaries, required engineer actions, or completion. Open every boundary report,
   gate request, and the completion response with the position line, printed by
   `dotnet <package-root>/validators/validate-plan.cs --position plan/<plan-id>` and
   pasted verbatim. Close a report that raised `needs-decision` findings with the decision
   interrogation — the structured question tool, one batch, recommendation and default per
   item — before parking anything as a register row.
