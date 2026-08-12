# Program Protocol

## Why this document exists

This defines the Program execution profile of Protodog, the Claude-native agentic execution protocol — for
engineer-selected, long-lived objectives that benefit from tracks, progressive planning, durable
cross-track state, responsibility dispatch, or capability-aware orchestration. It supersedes
`bundle/program-protocol.md` and depends on the [Foundation](foundation.md), the
[Verification and Assurance Policy](verification-and-assurance.md), and the
[Git Policy](git-policy.md); it redefines none of them.

Orchestration mechanics the predecessor specified in prose — writer isolation, context transfer,
bounded execution — are carried here by native subagent invocation and isolated worktrees; this
document keeps the invariants.

Status: committed execution profile, Claude-native line.

## Purpose and authority model

Every Program has one program plan and only the track plans current work requires. The program plan
is the authority for cross-track state; each track plan is the authority for execution within its
track. The profile creates no queues, journals, standing contracts, result files, or completion
reports.

**Write ownership is never ambiguous and never transferred away from coordination:**

- The coordinating agent owns the program plan, the Program worktree, and engineer-facing HIL at
  all times. It can therefore always persist a Program-level gate before requesting the engineer.
- Exactly one writer owns a track plan at a time; a concurrent track agent writes only its assigned
  track plan and returns cross-track state to the coordinator.
- Every concurrent repository writer works in its own isolated worktree (native isolation). There
  is no exclusive-write transfer of the Program worktree: work that must happen on the Program
  branch directly is done by the coordinating agent itself.
- An auditor edits neither the audited plans nor acceptance state.

## Entry and lifecycle

The engineer invokes the Program skill from sufficient current-session context, exact input or spec
references, or an existing program plan; overrides are skill arguments with the same defaults as
Task. Invocation records the profile, creates or resumes the one program plan (template-scaffolded,
validator-enforced), assumes the engineer-supplied Program worktree, grounds the Program, discovers
the known track topology and acceptance ownership, creates or resumes only the selected next track
plans, and proceeds without a plan-approval stop.

```text
grounding → Program planning → track planning → execution and verification
          → repository integration when applicable → integration reconciliation
          → repeat or complete
```

Audit is cross-cutting, selected at planning time against a declared placement, and pinned to exact
state at dispatch. Grounding and planning are not approval stops.

### Execution readiness

A Program is `ready` when its spec references are bound (immutable, enforced); the known
end-to-end track inventory and dependencies are represented; every applicable acceptance criterion
is assigned to a track or Program-level reconciliation; `Next` selects at least one track; and each
selected track has an execution-ready track plan. Other tracks stay `pending planning` — detail
plans are created just in time.

A track is `ready` when its spec references are bound, its assigned acceptance is mapped, the known
path appears as checkable blocks, its first eligible block is detailed into executable steps,
required verification is identified, and material gates are visible. Do not invent step detail past
unresolved uncertainty; use an exploration step.

## Program plan

A lean index and live cross-track state authority; canonical shape in
`protodog/templates/program-plan.md`.

```markdown
# <Program label>

- Profile: Program
- Status: pending planning | ready | in progress | blocked | completed | superseded | cancelled
- Cadence: interactive | continuous
- Spec context:
  - @specs/<document>.md
- Next: TRACK-01

## Contents

- [Tracks](#tracks)

## Tracks

| Track | Outcome | Track plan | Dependencies | Acceptance |
|---|---|---|---|---|
```

Rows follow the Foundation's plan row grammar — `- [ ] TRACK-01 · pending planning` in the `Track`
cell, no separate status column — and `## Contents` indexes every other section in document order.
It lists exactly the sections the plan has: the kernel above carries only `## Tracks`, so adding
**Program acceptance** or any optional live state below adds its entry at the same time.

`Next` names one selected track or an explicit release set — a resume pointer and interaction
selection, not a queue or concurrency declaration; omitted when nothing is eligible. The track
table is the inventory, dependency index, acceptance-ownership map, and scheduling authority; a
track-plan link appears only after that plan exists. `Grounding source` may temporarily replace
`Spec context` while `pending planning`, exactly as in Task.

Add a **Program acceptance** section only for criteria requiring cross-track reconciliation;
evidence stays in owning track plans. Optional live state, only when applicable: **Gates**,
**Decisions**, **Authority**, **Execution topology**, **Active assignments** (outstanding
dispatches), **Program issue ledger**, **Material notes**. No event log, delta log, branch
inventory, or duplicate acceptance matrix.

## Track plan

Canonical shape in `protodog/templates/track-plan.md`:

```markdown
# <Track label>

- Program plan: @<program-plan>
- Track: TRACK-01
- Status: ...
- Spec context:
  - @specs/<document>.md
- Cadence override: interactive | continuous   (omit when inheriting)
- Next boundary: STEP-01                       (interactive cadence only)

## Contents

- [Blocks](#blocks)
- [BLOCK-NN — <label>](#block-nn--label)
- [Acceptance](#acceptance)

## Blocks

| Block | Checkable result | Dependencies | Acceptance |
|---|---|---|---|

## BLOCK-NN — <label>

| Step | Checkable result | Affected surfaces | Acceptance | Verification |
|---|---|---|---|---|

## Acceptance

| Criterion | Status | Evidence |
|---|---|---|
```

Tracks are Program-scoped; blocks and steps are track-scoped; identifiers are stable after
execution starts. Block and step rows use the same identifier-cell grammar, and each block detail
section appears in `## Contents` as it is added. Track plans use the Task plan's optional state
forms where applicable — including the gates table, whose settled rows strike their identifiers —
and never copy Program-level state. Plans are live state, not transcripts.

## Status and progressive planning

Program, track, block, and step use the shared statuses with the shared aggregation rules: a
blocked descendant does not block its parent while independent work is eligible; terminal status
changes only to correct explicit error. Progressive planning updates existing plans directly under
new stable identifiers. A change to execution path or sequencing needs no approval by itself; a
material engineer-owned matter follows its Foundation gate; a change to bound outcome, scope,
acceptance, or intent requires superseding spec context and re-evaluation.

## Execution topology

Sequential by default: at most one active unit per applicable level. Declare concurrency only when
the plan records the exact work IDs, dependency independence, write surfaces and interference,
independent verification, and the reconciliation owner and check. Declaration establishes logical
eligibility only; repository policy, resource and cost authority, and host capability determine
actual concurrency. Concurrent repository writers each get an isolated worktree and reconcile into
the Program branch serially under the Git policy. Topology changes neither scope, authority,
acceptance, nor cadence.

## Responsibilities and capability

Responsibilities attach to assignments, not permanent agents: **planner** (creates and revises
plans; performs audit challenge), **executor** (implements and verifies bounded work),
**orchestrator** (coordinates planned work and live cross-track state), **auditor** (independently
produces findings). One agent may combine planner, orchestrator, and executor; audit independence
is mandatory — an auditor neither audits its own work nor performs its own challenge.

Model routing follows the Foundation: agent-type definitions own identities and tiers; select the
lowest-cost configuration demonstrated capable; audit challenge and every resulting remediation
step use the top-model configuration. Capability escalation without HIL requires existing resource
and cost authority; otherwise it becomes an authorization gate.

## Dispatch

A dispatch is an immutable assignment that references rather than copies authoritative context.
Bounded within-session subagent work needs no persisted file — the native invocation carries the
handoff content. Persist `DISPATCH-NN.md` when responsibility moves across sessions or agents
beyond a native call, or when the assignment grants isolated-writer repository access.

```markdown
# DISPATCH-NN — <checkable result>

- Program plan: @<program-plan>
- Track plan: @<track-plan>
- Assigned scope: ...
- Checkable result: ...
- Artifact and state references: ...
- Write boundaries: ...
- Required verification: ...
- Repository access: read only | isolated writer — <worktree>, <branch>, <baseline>
- Authority: ...
- Stopping conditions: ...
- Return path: coordinating Program agent
```

There is no coordination-writer access mode: the Program worktree is never write-transferred, so
the coordinating agent retains plan-write and HIL capability while any dispatch is outstanding.
While responsibility is outstanding, the owning plan lists the dispatch under active assignments.
The recipient returns concise results, evidence, gates, and material deltas through the return
path; the coordinator reconciles them into the applicable plans and removes the assignment. A
dispatched agent routes engineer escalations through the coordinator — track-scoped gates persist
in the track plan it owns; Program-level gates persist via the coordinator, which always can. No
dispatch-status machinery or separate result artifact exists.

## HIL and cadence

Programs start `interactive`; Program cadence is the default with track and block overrides — the
most-specific active setting wins and expires with its scope. The first interactive boundary
normally follows a small, reversible, independently verified execution unit unless the engineer
selects Program planning, track planning, audit, integration, or another transition. A
Program-level boundary may release one track or an explicit set and proceed through their planned
descendants until members complete and reconcile; gates and stop conditions always interrupt. The
coordinating agent owns engineer-facing HIL, batches related requests, and continues independent
authorized work while a gate blocks only dependent work. Checkpoint commits inside managed
worktrees follow the Git policy regardless of cadence.

Under `continuous` cadence — at Program, track, or block scope — apply the Foundation's
plan-currency rule against the plan that owns the transition: a step or block transition updates
its track plan, and cross-track state, track status, dependencies, and `Next` update the program
plan. Each update is verified against the persisted artifact before dependent work proceeds. A
track agent verifies its own track plan and returns cross-track deltas through its return path; the
coordinator's reconciliation of those deltas into the program plan is itself such a transition. A
continuous Program that leaves plans trailing execution has no boundary report to correct them and
nothing durable for resumption to read.

## Resumption, failure, and profile replacement

Resume from the program plan, selected track plan, bound specs, repository state, and any active
dispatch — native session continuity plus canonical state; no bootstrap artifact. Apply the common
recovery policy: a cross-track failure reconciles through the program plan; a track-local failure
stays with its step or track. The issue ledger is not a live failure log. Profile replacement
follows the Foundation: persist the gate, recommend, never self-switch; `superseded` only when an
exact successor exists.

## Track closure and the program issue ledger

A track is `completed` only when every assigned acceptance criterion is `satisfied` or has an
engineer-approved `accepted exception`; required verification has evidence; no gate blocks
closure; and unresolved Program-relevant items are deduplicated into the program issue ledger:

```markdown
## Program issue ledger

- ISSUE-NN — <concern>
  - classification: deferred issue | accepted gap   (only when meaningful)
  - origin: TRACK-NN, <evidence reference>
  - consequence: ...
  - revisit or reopening condition: ...
```

Resolved findings remain linked track evidence and are not copied into the ledger. Track completion
does not claim repository integration.

## Repository integration and completion

The engineer-supplied Program branch is the coordination state and the landing unit. Sequential
track work changes it directly through the coordinating agent; isolated-writer results integrate
into it serially under the Git policy's reconciliation sequence. After each relevant combined state
exists, integration reconciliation verifies it, records provenance, updates cross-track
acceptance, dependencies, baselines, and the ledger, and selects next work.

A Program is `completed` only when every bound acceptance criterion is `satisfied` or excepted;
every relevant track is terminal; required cross-track verification and integration reconciliation
have evidence; materially omitted verification is disclosed; no gate blocks completion; and
unresolved deferred issues and accepted gaps are disclosed. Cancelling a track does not discard its
acceptance ownership — affected criteria are reassigned, removed through superseding spec context,
or closed by accepted exception. The whole Program lands as one commit on `main` through the
engineer-triggered mechanism; completion does not imply landing.

## Assurance and audit

Apply the common assurance policy: each track applies the Task assurance policy to its assigned
result; dependent tracks verify consumption seams; independently produced work is verified after
reconciliation; completion establishes acceptance against the combined integrated state. Program
selection alone requires no additional heavyweight assurance.

Audits are selected at planning time with a declared track- or Program-wide placement, appear in
the owning plan, and are pinned to exact state by their dispatch when execution arrives. Launch
follows the assurance policy's authorization gate and guarded launcher; cycles number within their
scope and never overwrite. A track audit references its track plan, bound specs, and applicable
program-plan context; a Program-wide audit names every plan in scope. The planner performs the
challenge on the top-model configuration and may reconcile findings within existing scope and
granted authority; a disposition that changes outcome, scope, acceptance, or authority, or creates
an accepted gap, returns to the engineer through its gate. Confirmed work becomes track-plan steps
or blocks; re-audit follows the common triggers. If audit findings prove a recorded terminal status
invalid, changing it is an explicit correction of state.
