# Task Protocol

## Why this document exists

This defines the bounded-work execution profile of Protodog, the Claude-native agentic execution protocol.
It supersedes `bundle/task-protocol.md` and depends on the [Foundation](foundation.md), the
[Verification and Assurance Policy](verification-and-assurance.md), and the
[Git Policy](git-policy.md); it redefines none of them.

Status: committed execution profile, Claude-native line.

## Purpose

Use the Task Protocol for an engineer-selected, bounded engineering objective. One agent can carry
the whole lifecycle; bounded subagent use is permitted without importing Program tracks, blocks,
dispatches, or ledgers. Every Task has one durable task plan — the authority for live execution
state — which references rather than duplicates its spec context.

## Entry and lifecycle

The engineer invokes the Task skill from sufficient current-session context, exact input or spec
references, or an existing task plan. Overrides — execution scope, existing plan, cadence,
additional authority, assurance selection — are skill arguments; omitted values take defaults and
never become questions. Invocation:

1. records Task as the engineer-selected profile;
2. creates one task plan from the template (in `pending planning`) or resumes the matching plan —
   exactly one match resumes; none creates; multiple plausible matches use the Foundation HIL
   interface instead of guessing;
3. assumes the engineer-supplied worktree, inspects its repository state and policy, and grounds
   the work, creating or refining spec context as needed;
4. plans the known path end to end; and
5. proceeds through authorized work without a plan-approval stop.

```text
grounding → planning → execution and verification → completion
```

Grounding and planning are not approval stops; the engineer may select any transition as a declared
interaction boundary and may intervene at any time. Resumption preserves engineer-maintained
content and reconciles intervening repository changes; a material conflict becomes its gate.

### Execution readiness

A Task is `ready` when its spec references are bound (immutable from that point, enforced); every
applicable acceptance criterion has a stable ID and is mapped in the plan; the known path is
planned as checkable steps; the first step is executable; required verification — including any
planning-time-selected audit and its placement — is identified; and material gates are visible. A
genuine uncertainty may remain as an exploration step; an unspecified promise to plan later does
not make a Task ready.

## Task plan

One Markdown format at every Task size: a one-step Task is one row; a larger Task adds rows and
only the optional sections it needs. The canonical shape is `protodog/templates/task-plan.md`,
scaffolded at creation and enforced on write by the plan validator — empty placeholder sections,
unknown statuses, unmapped acceptance references, and renumbered IDs are rejected mechanically.

### Kernel semantics

```markdown
# <Task label>

- Profile: Task
- Status: pending planning | ready | in progress | blocked | completed | superseded | cancelled
- Cadence: interactive | continuous
- Spec context:
  - @specs/<document>.md
- Next boundary: STEP-01

## Contents

- [Steps](#steps)
- [Acceptance](#acceptance)

## Steps

| ID | Checkable result | Affected surfaces | Acceptance | Verification |
|---|---|---|---|---|

## Acceptance

| Criterion | Status | Evidence |
|---|---|---|
```

Rows follow the Foundation's plan row grammar: `- [ ] STEP-01 · ready` carries checkbox,
identifier, and status in the `ID` cell, so there is no separate status column. `## Contents` lists
every other section in document order and is updated whenever a section is added or removed.

While `pending planning`, `Grounding source` may temporarily replace `Spec context` with exact
input references and the concise invocation context needed for resumption; it is removed once spec
context is referenced. Spec context documents assign stable acceptance IDs before binding; the plan
references IDs and never restates criteria. `Next boundary` is present only under `interactive`
cadence when a transition or eligible unit can be released; it names one transition, one step, or
an explicit step set, and `go`/`next` releases exactly that. Step IDs are stable after execution
begins; a materially different replacement supersedes and links.

### Optional live state

Add only sections that carry applicable state: **Gates**, **Decisions**, **Authority** (only
engineer grants not carried by enforced configuration), **Assumptions and open questions**,
**Deferred issues and accepted gaps**, **Execution topology** (only when concurrency is selected),
**Material notes** (concise, dated, only when the reason for a change matters for resumption or
audit). There is no event log, attempt log, or completion-report artifact. Keep evidence concise
and referential: the command or check, material result, repository-state provenance when material,
and a link to retained larger output.

**Gates.** A gate is `open` or `settled`, and a settled gate strikes its identifier:

```markdown
## Gates

| ID | Gate | Owner | Blocked work | Closure |
|---|---|---|---|---|
| - [x] ~~GATE-01~~ · settled | authorization — audit launch | engineer | STEP-04 | granted 2026-08-12; DECISION-01 |
| - [ ] GATE-02 · open | decision — retry backoff | engineer | STEP-06 | engineer selects strategy |
```

The `Gate` cell names the gate type — `decision`, `authorization`, or `evidence` — and its subject;
`Closure` states the condition that settles it, and once settled, the outcome reference. Striking
the identifier settles it visibly while other gates remain open, so a reader sees at a glance what
still blocks work. The section is removed once no gate remains open and every outcome is persisted
where it belongs — a decision under **Decisions**, a grant under **Authority**, an exception in the
acceptance table. Striking is the settled marker, not the archive: the plan is live state, not a
gate history.

## Execution

Task status derives from executable state using the shared statuses. A blocked step does not block
the Task while independent steps remain eligible.

**Sequencing.** Execution is sequential by default: at most one step `in progress`, table order
supplying dependency order unless the plan says otherwise. A plan may select concurrent steps only
when it records dependencies, write surfaces, verification, and reconciliation. The coordinating
agent remains sole writer of the task plan and of the Task worktree; subagents are
repository-read-only by default and receive bounded work through native subagent invocation with
the handoff content the Foundation defines. When concurrency needs multiple repository writers,
each writer runs in a native isolated worktree; the coordinating agent reconciles results into the
Task worktree one at a time under the Git policy before dependent execution proceeds.

**Checkpoints.** Checkpoint commits inside the managed Task worktree are the agent's job under the
Git policy, at coherent verified units, regardless of cadence. Cadence governs when the engineer is
consulted, not who commits.

**Plan changes.** Update live state when evidence changes sequencing or execution; do not request
approval because the plan changed. A material engineer-owned choice, permission, or engineer-held
fact becomes its gate. If execution falsifies a bound spec premise or changes intended outcome,
scope, or acceptance, create a superseding spec; affected work waits until the plan explicitly
adopts it and is re-evaluated.

**Failure recovery.** Apply the common evidence-driven recovery policy: another attempt requires
remediation, new evidence, a new hypothesis, or a relevant state change. Triage every material
failure — remediate in scope, escalate capability, route an engineer escalation through its gate,
or record and continue when non-blocking. The Task has no issue ledger; deferred issues and
accepted gaps live in the plan.

## HIL and cadence

Every Task starts `interactive` unless invocation selects `continuous`. The first interactive
boundary normally follows a deliberately small, reversible, independently verified execution unit;
the engineer may instead select grounding, planning, review, or another meaningful transition. An
ordinary boundary report states completed work, material evidence, and the next declared unit; an
actual gate uses the fuller decision-ready Foundation request. Cadence is Task-wide; the engineer
may switch it explicitly at any time, and `go` releases one declared boundary without changing
cadence or authority.

Under `continuous` cadence, apply the Foundation's plan-currency rule: with no boundary report to
surface it, the task plan is the engineer's only view of progress. Each step transition — including
the acceptance, gate, and decision outcomes it settles — updates the task plan, and that update is
verified against the persisted artifact before the next step starts. Checkpoint commits already
happen at coherent verified units regardless of cadence; plan currency is the state half of the
same discipline.

## Resumption and profile replacement

The task plan, bound specs, and repository state are the resumption contract — no bootstrap or
resumption artifact exists. Persist a handoff only when responsibility transfers with material
context that cannot live in that state. If evidence indicates the profile is unsuitable, persist
the decision gate and present a decision-ready request; the Task cannot switch profiles. On
replacement, reconcile reusable verified evidence without transferring scope, authority,
acceptance, or bindings automatically; the original becomes `superseded` only when its exact
successor exists.

## Completion

A Task is `completed` only when every bound acceptance criterion is `satisfied` or has an
engineer-approved `accepted exception`; required verification has evidence; materially omitted
verification is disclosed; and no open gate blocks completion. The final response summarizes
acceptance, material evidence, omissions, retained issues or gaps, and landing state when material
— it is not persisted as a separate report. Completion is not landing; when landing occurs, the
whole Task is one commit on `main` through the engineer-triggered mechanism.

## Assurance and audit

Apply the common assurance policy. Task selection alone never requires a full test-suite run,
evaluation run, review, audit, or independent executor. Step evidence is reusable only while later
changes leave its claim and target state unaffected; completion establishes acceptance against the
final Task state.

An audit exists only when selected at planning time (by the engineer, spec context, repository
policy, or governing assurance policy) and appears in the plan with its declared placement. When
execution reaches that placement, the agent persists the self-contained dispatch pinning the exact
state, and launch follows the assurance policy's authorization gate and guarded launcher. The
numbered cycle is `audit-NN-dispatch.md`, `audit-NN.md`, `audit-NN-challenge.md` under
`plan/<plan-id>/audits/`.

**Challenge ownership.** In a Task, the coordinating Task agent owns the audit challenge and all
resulting remediation, executing on the top-model configuration — routed through the pinned
challenge agent type when the session model is not that tier. The auditor never challenges its own
audit. Confirmed findings become ordinary task-plan steps with affected verification; no separate
fix plan exists.

## Worked example: minimum one-step Task

```markdown
# Correct retry configuration example

- Profile: Task
- Status: ready
- Cadence: interactive
- Spec context:
  - @specs/retry-example.md
- Next boundary: STEP-01

## Contents

- [Steps](#steps)
- [Acceptance](#acceptance)

## Steps

| ID | Checkable result | Affected surfaces | Acceptance | Verification |
|---|---|---|---|---|
| - [ ] STEP-01 · ready | Example uses the supported retry option and renders correctly. | `docs/retries.md` | ACCEPTANCE-01 | Build docs; inspect rendered example. |

## Acceptance

| Criterion | Status | Evidence |
|---|---|---|
| ACCEPTANCE-01 | pending | — |
```

Nothing else is required — no gates, decisions, topology, or completion-report sections, and the
contents index stays two entries long because the plan has two sections. A Task needing optional
machinery adds exactly the sections that carry state and lists them in `## Contents`, as in the
gates example above; features appear because the Task needs them, not as a checklist.
