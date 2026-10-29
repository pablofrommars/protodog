---
name: coordinate
description: Dispatch a runbook's units from one session as isolated-worktree subagents, rule within the runbook's standing rulings, re-issue the runbook on evidence or instruction, and route deferred items downstream. Engineer-invoked; Claude Code only.
disable-model-invocation: true
argument-hint: [@inputs/runbook.md] [units: ...] [cadence: ...] [authority: ...] [assurance: ...]
---

# Coordinate a runbook

You are dispatching a runbook's units from one session. Coordination is one dispatch option
beside hand dispatch, not the multi-unit shape: the runbook remains the engineer's cut, each unit
is a Task that runs under `/protodog:task` in its own session, and the plan, the spec, the Git
policy, and the hooks govern it unchanged. This skill is not a Protocol entry point; it creates
no plan of its own. It is Claude-native: it needs isolated writer worktrees, which the Codex
adapter does not support, so the Codex package does not expose it.

Load `core/host-runtime.md`, `core/foundation.md`, `core/task-protocol.md`, and
`core/git-policy.md` from the package root (`../..` from this file's directory) before
dispatching: the routing ladder, the HIL interface, the worktree rule, and the engineer-owned Git
operations all bind here.

## Arguments

Stated in plain language inside the invocation, only when they differ from defaults:

- The runbook: an exact `@inputs/...` reference, else the runbook drafted in the visible session,
  else exactly one runbook under `inputs/`; several candidates become a question, never a guess.
- `units: <ids>` — the subset to dispatch. Default: every unit whose prerequisites are landed, in
  runbook order, parallel where the runbook says so.
- `cadence:`, `authority:`, `assurance:` — passed through to every dispatched unit on top of what
  its dispatch prompt already carries. Dispatched units run `continuous` unless overridden,
  because a subagent cannot ask the engineer: its gates persist in its plan and end its turn.

Model preferences are not arguments. The unit's tier comes from the runbook under the routing
ladder; the coordinator may escalate a rung on evidence and reports it, and never downshifts
below the runbook's tier.

## Procedure

1. **Position.** Read the runbook. Inspect HEAD, branches, worktrees, `plan/`, and
   `plan/deferred.md`. Classify every unit: landed (its commit is on `main`, or on the declared
   line's branch under the line policy), in flight (a plan exists and is not terminal), eligible
   (selected, prerequisites landed, not started), blocked (and on what). Open with that
   classification and the standing rulings in force; a runbook older than the repository state
   it describes is re-issued first (step 6).
2. **Dispatch.** One native subagent per eligible unit, in an isolated worktree inside the
   repository's writable roots — never a sibling directory — with the model the unit's rung names
   on this host; a fork inherits the session model and cannot be downshifted, so use a fresh
   agent when the rung is lower. The prompt is the unit's dispatch prompt verbatim, plus the
   pass-through overrides, plus the instruction that the plan is the only progress view and that
   a gate ends the turn with the gate persisted. Parallel dispatch only for units the runbook
   marks parallel, with disjoint write boundaries.
3. **Rule.** When a subagent's turn ends, read its plan and print the position line from the plan
   validator. Then: completed — go to step 4; a gate inside a standing ruling — rule it, have the
   ruling persisted in the plan with the provenance `ruled by coordinator under <ruling>`, and
   resume the subagent; a gate outside — hold it; failure beyond recovery — report it. Gates held
   from all units are put to the engineer as one decision interrogation, the structured question
   tool with up to five requests per batch, recommendation first and default stated, and the
   affected units resume after the answer. Nothing resumes on silence or elapsed time.
4. **Land.** Under the per-unit policy a completed unit is ready for the engineer's landing: the
   rebase, the landing review, and the commit to `main` are engineer-executed actions, so report
   the unit as ready with its position line and wait. Under the line policy, merge the completed
   unit's branch into the line's branch — an ordinary merge, never a merge of `main` — and
   continue; the line lands by the engineer's hand once. After each landing, re-classify units.
5. **Route deferred items.** At every landing, read the landed unit's deferred items. Each goes to
   the downstream unit owning its surface: adopted in that unit's dispatch as a step
   `from <plan-id>` when the source closed it as carried, or `from D-NN` when it lifted to the
   register with that unit as its revisit. Items with no owner downstream stay parked, with the
   reason in the re-issued runbook.
6. **Re-issue.** Re-cut the runbook under the runbook skill's contract when evidence changes it —
   a landing moved prerequisites, a unit split, a cluster surfaced, a rung moved — or when the
   engineer instructs it: adding a unit, an audit, a constraint. Open the re-issue with what
   changed and why. Engineer-owned items — objective, landing policy, standing rulings,
   exclusions — hold until the engineer rules; a re-cut that would change the objective or
   acceptance is a decision gate, not an edit. A persisted runbook is rewritten in place. An
   audit added to the sequence still passes its authorization gate at launch; present that gate
   right after the re-cut so it costs one answer.
7. **Close.** Stop when every selected unit is landed or waits on the engineer. Report units
   dispatched, rulings given with their provenance, items routed, the runbook's current cut, and
   the worktrees left for the worktree survey — cleanup is engineer-owned.

## Boundaries

- Never commit to `main`, rebase, fetch, pull, push, merge `main` into a branch, or remove a
  worktree; the hooks deny these and a denial is never retried.
- Never dispatch a unit whose prerequisites are not landed, pass a surface grant the runbook does
  not carry, or rule a gate the standing rulings do not cover.
- Never treat a subagent's report as the engineer's word: approvals, grants, and releases come
  from the engineer in this session only.
