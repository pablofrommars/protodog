---
name: coordinate
description: Dispatch a runbook's units from one session as isolated-worktree subagents on the rung each phase fits, manage their Git from worktree to landing, rule within the runbook's standing rulings, re-issue the runbook on evidence or instruction, and route deferred items downstream. Runs from the primary checkout or from a worktree. Engineer-invoked; Claude Code only.
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

Units are for code that lands. Exploration — probing how a provider or a system behaves, where
each finding sets the next question — is not dispatched as a unit: it runs in this session,
without a plan or spec, under a spend bound the engineer has ruled, with every paid call
described to the engineer before it is sent, a log of what was sent and found, and a pause after
every one to three probes and at the end.

Load `core/host-runtime.md`, `core/foundation.md`, `core/task-protocol.md`, and
`core/git-policy.md` from the package root (`../..` from this file's directory) before
dispatching: the routing ladder, the HIL interface, the worktree rule, and the Git policy's
coordination delegation all bind here. Then read the repository's contributing guide
(`CONTRIBUTING.md`, or the document repository instructions name) for its Git workflow and
guarded script.

## Git is the coordinator's

Invoking this skill delegates the coordination's Git to you: unit worktrees and branches, merges,
synchronization, squash, landing, and cleanup. The engineer is not asked to perform or trigger
them. `no landing` in the invocation withholds landing alone; a withheld landing is reported as
ready and waits.

- **The guarded script is the path.** Where the contributing guide documents a guarded Git
  script, every operation it covers goes through it, run from the worktree the guide says it
  runs in, with its interactive prompts answered from the plan (the title is the first
  checkpoint's subject, kept unless the delivered work drifted from it; a line lands under the
  runbook's objective). Raw Git covers only what the script does not: local inspection, unit worktrees and
  branches it cannot base where the unit binds, and ordinary merges between unit and line
  branches. A hook denial is never retried or routed around; an operation neither the script
  nor the hooks allow is reported and waits for the engineer.
- **From the primary checkout on `main`** you never write there: every unit works in its own
  worktree. Under the per-unit policy each unit's branch starts from current `main` and lands
  alone. Under the line policy you first provision the line's branch in its own worktree.
- **From a worktree** its branch is the line unless the runbook says otherwise: units branch
  from its HEAD and merge back into it, and you are the only writer in it — merges and the
  runbook, no unit work.
- **A unit lands only when it is done**: its plan is terminal, its selected assurance is closed,
  and no gate is held. A landing does not run the register review; reviews run where the
  runbook schedules them (step 5). Verification the landing rewrites — a rebase that moved the base — is rerun before the
  push; a rejected push is inspected and repeated through the script, never forced.
- **After a landing**, bring your own checkout up to date through the script before provisioning
  the next unit, so each unit binds against what landed.

## Arguments

Stated in plain language inside the invocation, only when they differ from defaults:

- The runbook: an exact `@inputs/...` reference, else the runbook drafted in the visible session,
  else exactly one runbook under `inputs/`; several candidates become a question, never a guess.
- `units: <ids>` — the subset to dispatch. Default: every unit whose prerequisites are landed, in
  runbook order, parallel where the runbook says so.
- `cadence:`, `authority:`, `assurance:` — passed through to every dispatched unit on top of what
  its dispatch prompt already carries. Dispatched units run `continuous` unless overridden,
  because a subagent cannot ask the engineer: its gates persist in its plan and end its turn.

Model preferences are not arguments. Rungs follow the routing ladder by phase, as the
Foundation sets them: a unit plans and executes on different rungs, never one model for the whole
Task. Planning runs on the top rung for every unit, and execution runs on the rung the unit's
readiness report recommends. The runbook's tier is the cut's estimate of that execution
rung, not a floor on it. The coordinator may escalate a rung on evidence and reports it, and never
runs execution below the readiness recommendation. A model the engineer names for a unit, in the
runbook or in this session, is that unit's rung for the phase named.

## Procedure

1. **Position.** Read the runbook. Inspect HEAD, branches, worktrees, `plan/`, and
   `plan/deferred.md`. Classify every unit: landed (its commit is on `main`, or on the declared
   line's branch under the line policy), in flight (a plan exists and is not terminal), eligible
   (selected, prerequisites landed, not started), blocked (and on what). Open with where you
   sit — primary checkout or worktree, and the branch units bind against — that classification,
   the model fit of each eligible unit (planning rung, estimated execution rung, confidence), the
   guarded script in force, and the standing rulings; a runbook older than the repository state
   it describes is re-issued first (step 6).
2. **Dispatch.** One native subagent per eligible unit, in an isolated worktree you provision
   from the branch the unit binds against — inside the repository's writable roots, or the
   managed root of the guarded script when it provisions — on the top rung: the dispatch plans
   the unit, and planning through execution readiness belongs on the top-model configuration. The
   prompt is the unit's dispatch prompt verbatim, plus the pass-through overrides, plus the
   instruction that the plan is the only progress view and that a gate ends the turn with the
   gate persisted. Parallel dispatch only for units the runbook marks parallel, with disjoint
   write boundaries.
3. **Rule.** When a subagent's turn ends, read its plan and print the position line from the plan
   validator. Then: readiness — the plan is `ready` and the turn ended at the readiness boundary:
   take the handoff's execution-model recommendation as the unit's execution rung, escalated on
   evidence and never lowered, and relaunch execution as a fresh agent on that rung — never a
   fork, which inherits the session model — in the same worktree, with the handoff's resumption
   invocation (`/protodog:task plan: @plan/<plan-id>/task-plan.md` plus the unit's overrides,
   `cadence: continuous` among them), the same dispatch instructions, and the instruction to end
   the turn, next boundary persisted, before any step the plan marks with a capability escalation
   above its rung; when a turn ends there, the remainder is relaunched the same way on the
   escalated rung, and stays there: nothing relaunches it back down. A plan whose steps alternate
   rungs therefore runs as one agent on the highest rung its steps name, the only writer of the
   plan and the worktree, handing bounded mechanical batches to subagents on the lower rung; no
   step then runs below the rung the recommendation gave it. Its dispatch carries the instruction
   not to end a turn while a subagent of its own is still running, and, when a turn is forced to
   end first, to persist in the plan what is still running and mark its report interim; the next
   dispatch after an interim report first establishes whether that work finished. A rung that
   moved from the runbook's estimate is reported and re-issues the
   runbook (step 6). Completed — go to step 4; a gate inside a standing ruling — rule it, have the
   ruling persisted in the plan with the provenance `ruled by coordinator under <ruling>`, and
   resume the agent whose turn ended, on its phase's rung; a gate outside — hold it; failure
   beyond recovery — report it. Gates held
   from all units are put to the engineer as one decision interrogation, the structured question
   tool with up to five requests per batch, recommendation first and default stated, and the
   affected units resume after the answer. Nothing resumes on silence or elapsed time. A ruling
   the engineer gives while an agent is still running is relayed to it as a message only when it
   stays inside the authority the agent already holds — its scope, write surfaces, and paid
   bounds; a ruling that grants authority reaches the unit in a dispatch or relaunch invocation,
   where the Foundation places grants under `continuous`, and an agent that receives one as a
   message records it and ends its turn. Either way the plan carries it with the provenance
   `ruled by engineer, relayed by coordinator`.
4. **Land.** Under the per-unit policy, land each completed unit on `main` through the guarded
   script and report the commit. Under the line policy, merge the completed unit's branch into
   the line's branch — an ordinary merge, never a merge of `main` — rerun the verification the
   combined state affects, and continue; when every selected unit is merged and the
   objective-level completion holds, land the line once. Without a guarded script, or under
   `no landing`, report the unit or line as ready with its position line and wait. After each
   landing, re-classify units.
5. **Route deferred items.** At every landing, read the landed unit's deferred items. Each goes to
   the downstream unit owning its surface: adopted in that unit's dispatch as a step
   `from <plan-id>` when the source closed it as carried, or `from D-NN` when it lifted to the
   register with that unit as its revisit. Items with no owner downstream stay parked, with the
   reason in the re-issued runbook. Run a register review only where the runbook schedules one:
   dispatch `deferred-review` with the scheduled focus as a fresh subagent in its own worktree,
   on the branch units bind against and between landings, never while a unit that touches the
   register is in flight. Its proposals and `needs-decision` rows join the held gates as one
   interrogation; the ratified transitions are applied there and the review's worktree merges
   or lands like a unit. A transition that shows the register moved under the sequence — a
   closed row a unit was to adopt, a fired trigger — re-issues the runbook (step 6), which may
   schedule a further review.
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
   the commits landed, and the worktrees cleaned up through the guarded script or left for the
   worktree survey. Close with one cost
   line per unit: the rung planning and execution ran on against the runbook's estimate, agents
   launched, wall clock across its turns, gates ruled and held, and the verification runs its
   completion response reports — so the tiering outcome is read per unit, not reconstructed.

## Boundaries

- Never reach `main` or the remote with raw Git — commit to `main`, rebase, fetch, pull, push,
  remove a worktree — and never merge `main` into a branch: these go through the guarded script
  or not at all; the hooks deny them and a denial is never retried.
- Never land a unit that is not done, force a push, or edit a unit's work from your own
  checkout.
- Never dispatch a unit whose prerequisites are not landed, pass a surface grant the runbook does
  not carry, or rule a gate the standing rulings do not cover.
- Never plan a unit below the top rung, run its execution below the readiness recommendation, or
  carry an agent across the readiness boundary on a rung the recommendation did not name.
- Never treat a subagent's report as the engineer's word: approvals, grants, and releases come
  from the engineer in this session only.
