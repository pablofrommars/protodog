# Protodog — Foundation

## Why this document exists

This is the common normative layer for Protodog, the Claude-native agentic execution protocol. It defines
the concepts, authority boundaries, lifecycle, human-in-the-loop interface, state semantics, and
artifact contracts shared by the Task and Program execution profiles. A profile may add machinery
but may not redefine this Foundation.

It supersedes `bundle/protocol-foundation.md`. The semantic essence is unchanged; what changed is
the enforcement model: mechanics that the vendor-neutral predecessor specified in prose are now
carried by the installed harness (skills, subagents, isolated worktrees) and by deterministic
enforcement (hooks, validators). Where an invariant is mechanically enforced, prose states the
intent and the enforcement defines the operational boundary; prose-only rules remain fully binding.

Status: committed normative Foundation, Claude-native line.

## Normative set and layering

| Layer | Artifacts | Role |
|---|---|---|
| Semantic core | this document, `task-protocol.md`, `program-protocol.md`, `verification-and-assurance.md`, `git-policy.md` | Meaning, authority, lifecycle — host-neutral prose |
| Templates | `protodog/templates/` | Canonical artifact shapes; scaffolded at creation |
| Enforcement | `protodog/hooks/`, `protodog/validators/` | Deterministic checks; authoritative for what they decide |
| Entry | Task and Program skills | Implement the invocation contract |
| Prompt registries | `prompt-utility.code-snippets`, `boxer.code-snippets` under `prompt-registry-style.md` | Host-portable prompts outside Protocol state |
| Ideation mechanics | `ideation-audit` skill | Claude-native adversarial pressure for ideation sessions — outside Protocol state |
| Canvas mechanics | `working-canvas` skill | Claude-native maintenance of the working canvas for diagram-suited sessions — a working view, outside Protocol state |
| Maintenance mechanics | `plan-sweep`, `deferred-review` skills with `scripts/sweep-check.cs` | Engineer-confirmed retirement and register upkeep |

This table is the complete normative closure; nothing outside it carries Protocol authority. A
validator rejection outranks a prose interpretation for whatever the validator decides; disputes
about a validator's correctness are resolved by fixing the validator, not by overriding it ad hoc.

## Profiles and selection

The engineer selects Task or Program by invoking its skill from sufficient current-session or
persisted context. The Protocol does not score the choice, select automatically, or promote a Task
when Program-like conditions appear. A pre-entry recommendation may be requested — the registry
protocol utility (`p-protocol-recommend-profile`) or feature ideation's profile orientation — and
is advisory only: never solicited by an entry skill, never a selection; the engineer selects by
invocation. Ideation happens outside the Protocol — through the external
engine or a portable ideation mini protocol; inside a Claude ideation session, the
`ideation-audit` skill may dispatch adversarial subagent audits of the session's settled
conclusions, which remain session material. Ideation results enter the Protocol only as `inputs/`
documents with cited provenance.

If later evidence makes the selected profile unsuitable, the agent presents a decision-ready
recommendation. Only the engineer may choose a replacement. The original plan becomes `superseded`,
links its exact successor, and transfers no scope, acceptance, authority, or spec binding
implicitly. Task vocabulary is a conflict-free subset of Program vocabulary; Program-only concepts
do not appear as empty Task fields.

## Design principles

1. **Minimize HIL friction, not engineering participation.** Supervised collaboration is the
   default; the engineer selects meaningful feedback points and may intervene at any time.
2. **Justify mandatory state.** Agent-facing convention is retained when it improves grounding,
   determinism, resumption, orchestration, or auditability — never for ceremony or symmetry.
3. **Communicate at engineer altitude.** Engineer-facing messages carry evidence, uncertainty,
   recommendations, alternatives, consequences, and authority needs in plain language.
4. **Prefer self-explanatory consistency.** One canonical term per material distinction.
5. **Consolidate purpose and persistence.** One term names an activity and its durable artifact
   when separating them changes no semantics.
6. **Ground claims.** Availability is not truth; capability is not authority; completion is not
   landing.
7. **Preserve profile proportionality.** Task must remain usable by one agent without Program
   machinery.
8. **Enforce over specify.** When an invariant is mechanically decidable, implement it as a hook or
   validator and delete the defensive prose. Prose is reserved for judgment.

## Context, specs, and artifacts

### Context progression

- An **input** is context explicitly persisted unchanged for planning under `inputs/`. It need not
  be grounded, consistent, or authoritative. External-engine material enters the Protocol only this
  way, with `source cited` provenance — never directly into `specs/` or `plan/`.
- A **spec context document** is reconciled engineering intent under `specs/`, produced during
  grounding and planning.
- A **plan** contains intended execution and live execution state. It references its bound specs
  rather than copying them.

When an execution-ready plan binds a spec and becomes `ready`, that spec is immutable (enforced).
Changed intent or a falsified premise creates a superseding spec; affected work adopts it
explicitly and is re-evaluated before execution.

### Artifact paths and the plan id

One repository-level artifact root defaults to the repository root and may be configured once by
the installed adapter. Invocations do not override it.

A **plan id** is the kebab-case slug of the profile label, unique under `plan/`; a collision is
resolved with an explicit numeric suffix. It is stable for the life of the plan, and every child
path derives from it.

| Purpose | Path |
|---|---|
| Supplied or generated input | `inputs/` |
| Curated spec context | `specs/` |
| Execution state | `plan/<plan-id>/` |
| Task plan | `plan/<plan-id>/task-plan.md` |
| Program plan | `plan/<plan-id>/program-plan.md` |
| Track plan | `plan/<plan-id>/tracks/TRACK-NN/track-plan.md` |
| Program dispatch | `plan/<plan-id>/dispatches/DISPATCH-NN.md` |
| Audit cycle (profile-wide) | `plan/<plan-id>/audits/` |
| Audit cycle (track) | `plan/<plan-id>/tracks/TRACK-NN/audits/` |
| Deferred register | `plan/deferred.md` |

An audit cycle consists of exactly these files, numbered within their declared scope:
`audit-NN-dispatch.md`, `audit-NN.md`, `audit-NN-challenge.md`.

### Plan membership, lift, and retirement

`plan/` holds exactly two kinds of entry: plan-id directories and the deferred register
(`plan/deferred.md`, shaped by its template and validated on write). Ideation products enter as
`inputs/`; rationale that must outlive its plan moves to spec context or repository
documentation. `plan/` is working execution state — never an archive, and never a decision record
in disguise.

A plan may not go terminal while it is the sole carrier of a live obligation. Before `completed`,
`superseded`, or `cancelled` is recorded, every deferred issue and accepted gap — and any open
question still unresolved, which becomes a deferred issue at that moment — either lifts to the
deferred register or records its closure through the owning profile's disposition-arrow grammar
(enforced). A lifted row is self-sufficient: it carries the condensed claim, why it matters, and
its revisit condition, because the source plan will be retired; its provenance cell points into
history, not at required reading.

Retirement deletes terminal, landed plan directories from the working tree. Git history on `main`
is the archive — one-commit landing guarantees the provenance — and a terminal plan is delete-safe
by construction: after the lift, nothing may cite it as ongoing authority. Retirement is
engineer-triggered, land-then-sweep; an agent never retires a plan directory on its own
initiative. Audit-report immutability forbids edits while the plan lives; it does not survive the
plan — retirement removes the directory whole.

### Canonical state and working views

Plans, specs, and audit artifacts in the repository are the sole authority for execution state.
Harness task lists, session memory, transcripts, and the working canvas — the session-scoped
rendered model a diagram-suited session may keep — are working views: state may be mirrored *to*
them for visibility, never *from* them, and harness-generated identifiers never appear in canonical
artifacts. Durable identifiers are readable and stable: `ACCEPTANCE-01`, `GATE-02`, `STEP-01`,
`BLOCK-01`, `TRACK-01`, `DISPATCH-01`, `DECISION-01`. Never reuse or renumber one after execution
begins; supersede and link instead.

Artifact ownership is `agent generated / engineer maintained / mixed / unclear`; mixed artifacts
assign ownership at section level. Preserve engineer-maintained content and reconcile intervening
changes rather than overwriting. Use compact tables for dense homogeneous indexes and labeled
lists for sparse optional state; omit absent optional sections (enforced).

### Plan row grammar and contents index

Every plan row that carries state — step, block, track, gate, acceptance criterion — puts its
status marker, identifier, and status label in one identifier cell, and no plan table has a
separate status column (enforced):

```text
⬜ STEP-01 · ready
🟡 STEP-02 · in progress
✅ STEP-03 · completed
🔴 ACCEPTANCE-02 · unmet
✅ ~~GATE-01~~ · settled
```

Five markers carry meaning, not status, and are reused across every row kind. A reader learns the
set once and scans any plan with it:

| Marker | Meaning | Work row | Acceptance criterion | Gate |
|---|---|---|---|---|
| ⬜ | not started | `pending planning`, `ready` | `pending` | — |
| 🟡 | active | `in progress` | — | — |
| ✅ | done | `completed` | `satisfied` | `settled` |
| 🔴 | needs attention | `blocked` | `unmet` | `open` |
| ⬛ | closed without being met | `superseded`, `cancelled` | `accepted exception` | — |

The mapping is many-to-one, so the marker narrows the state without naming it: the label beside it
is the precise status and the only thing to quote, grep, or reason from. Marker and label must
agree (enforced) — a row cannot claim `completed` under 🟡. The grouping is chosen so that the
distinction a reader acts on survives the collapse: 🔴 separates a criterion that was checked and
failed from one merely `pending`, and ⬛ separates work closed without being met from work
completed.

Markers are single codepoints requiring no variation selector. This is a constraint on the set, not
an aesthetic: a marker needing U+FE0F is invisible in most editors, silently dropped by ordinary
copy-paste, and would fail validation against a row that looks identical to a correct one.

Strikethrough marks a settled gate identifier and appears nowhere else. Work that ends `superseded`
or `cancelled`, and a criterion closed by `accepted exception`, say so through ⬛ and the status
label rather than by striking a durable identifier.

A spec's acceptance-criteria table is unaffected by this grammar: it defines criteria and assigns
their IDs, carries no status, and is immutable once bound.

Every plan opens with a `## Contents` section listing every other section as a link, in document
order (enforced). It is the plan's section index, so which optional live state a plan actually
carries is visible without reading it. Anchors follow the host's ordinary heading-slug convention
and are not themselves enforced.

## Shared lifecycle and work status

```text
optional external ideation → engineer invokes Task or Program
  → ground intent, inputs, repository, policy, and current state
  → curate and bind spec context → plan to execution readiness
  → execute and verify planned work
  → optional review or audit when selected at planning
  → satisfy acceptance or obtain an accepted exception
  → complete the profile → engineer-triggered landing
```

Work status at every level is one of: `pending planning`, `ready`, `in progress`, `blocked`,
`completed`, `superseded`, `cancelled`. `Completed`, `superseded`, and `cancelled` are terminal and
change only to correct explicit state error. Supersession and cancellation do not establish
acceptance; completion does not establish integration, landing, or shipping. A blocked descendant
does not block its parent while independent work remains eligible.

Grounding, plan creation, plan revision, successful verification, review, and status reporting are
not approval stops. The profile proceeds through authorized, unambiguous work until its cadence
boundary, a gate, a required engineer action, a stop condition, or completion.

## Authority and model routing

**Authority** is permission or decision ownership granted by the engineer or governing repository
policy. Tools, host permissions, model capability, cadence, plan state, and prior success do not
create it.

Profile invocation authorizes ordinary, reversible, in-scope local work allowed by repository
policy, including delegated Git operations exactly as bounded by the Git policy. It does not
authorize destructive, external, paid, difficult-to-reverse, or sensitive-data effects, nor
landing; those follow their gates.

**Model routing** lives in agent-type definitions, which own current model identities and effort
tiers. Select the lowest-cost configuration demonstrated capable of the assignment. Audit challenge
and every resulting remediation step use the top-model configuration. Routing to a more capable
model is a capability escalation: it preserves scope and authority and creates no permission. An
engineer escalation transfers a blocking material choice, authorization, or engineer-held evidence
through the matching gate.

## HIL interface

An agent asks the engineer only for:

| Interface | Why the engineer is needed | What waits |
|---|---|---|
| Profile invocation | The engineer owns profile selection. | Execution has not started. |
| Decision gate | A material choice belongs to the engineer. | Only dependent work. |
| Authorization gate | An understood action needs permission — including every audit launch (cost and data egress) and any external, paid, or hard-to-reverse effect. | The action and dependent work. |
| Engineer-held evidence | Required evidence or access only the engineer has. | Only dependent work. |
| Accepted exception | Completion is requested with unmet acceptance. | Completion. |
| Engineer-executed action | The Protocol assigns an action, such as landing. | Only dependent work. |
| Declared interaction boundary | The engineer selected a feedback point. | The next declared transition or unit. |

A material contradiction, spec change, or failure beyond recovery routes through a decision,
authorization, or evidence gate — there is no fourth gate type.

**Decision-ready requests.** Resolve everything available evidence can resolve first. The smallest
sound request states: the exact need; why it requires the engineer; the supported recommendation
unless evidence cannot distinguish choices; concise evidence, alternatives, and consequences; what
is blocked and what continues; and the shortest unambiguous response shape. Authorization requests
add the exact action and target, external and data effects, cost bound, recovery path, and grant
scope. Batch up to five related, independently answerable requests; continue independent authorized
work while a gate is open.

**Persistence and response.** Persist a blocking gate in its owning plan before requesting the
engineer — plan-write ownership is arranged so this is always possible (see the Program topology
rule). After the response, persist the exact decision, authority bound, evidence, or exception.
Silence is not approval; an ambiguous response does not close a materially ambiguous gate. `Go` or
`next` releases only the declared boundary and changes nothing else. A material change to target,
cost, reversibility, or consequences invalidates the corresponding part of a prior closure.

## Execution cadence and topology

Both profiles start `interactive`: proceed to the declared boundary, report completed work,
material evidence, and the next unit, then wait. `Continuous` is an explicit override that proceeds
through unambiguous planned work until a gate, stop condition, required engineer action,
completion, or engineer intervention. Cadence changes interaction frequency only. A Program has a
Program default with track and block overrides; the most-specific active setting wins and expires
with its scope. The engineer may intervene at any time.

**Plan currency under `continuous` cadence.** An interactive boundary forces the plan current — the
report is written from it, and the engineer sees the state. `Continuous` removes that boundary, so
the plan is the only place the engineer can observe progress and the only state resumption has.
Under `continuous`, writing the owning plan is therefore part of the transition itself, not
bookkeeping deferred to a convenient moment: on every status transition of a work row — and on
every acceptance, gate, or decision outcome that transition settles — update the plan and verify
the persisted artifact reflects the new state before dependent work proceeds. A transition whose
plan write cannot be verified is a failure to triage under the recovery policy, not a step to
continue past. This is a prose rule: no validator can see a transition, only the artifact it left
behind.

Execution is sequential by default. A plan selects concurrency only when it names the exact work,
dependency independence, write surfaces, independent verification, and reconciliation. Every
concurrent repository writer runs in its own isolated worktree (native subagent isolation); the
coordinating agent always retains its own worktree and its plan-write ownership — coordination
never cedes the ability to persist canonical state. Results reconcile serially under the Git
policy. Topology changes neither cadence nor authority.

## Core vocabulary

- **Outcome** — the observable result the work intends to produce.
- **Scope / out of scope** — included and explicitly excluded work, surfaces, and effects.
- **Constraint** — an imposed restriction on acceptable solutions or execution.
- **Invariant** — a condition that must remain true throughout or after the work.
- **Acceptance criterion** — a falsifiable completion condition:
  `pending / satisfied / accepted exception / unmet`. Only the engineer accepts an exception; a
  persistent consequence also becomes an accepted gap.
- **Assumption** — a provisional claim with the evidence that would settle it; blocking material
  assumptions become evidence gates.
- **Step** — the smallest planned unit expected to produce independently checkable progress. An
  **exploration step** reduces a named uncertainty and produces evidence or a decision.
- **Material** — capable of changing outcome, scope, acceptance, authority, safety, architecture,
  sequencing, or significant cost or blast radius. A **material delta** is such a change and its
  durable update in the owning plan; there is no separate delta log.
- **Open question** — unresolved and non-blocking; reclassify as a gate when blocking.
- **Decision** — a settled material choice with owner, rationale, and evidence.
- **Decision / authorization / evidence gate** — a blocking choice, permission, or fact, with
  owner, affected work, and closure condition.
- **Deferred issue** — postponed work with reason and revisit condition.
- **Accepted gap** — a retained limitation with rationale, consequence, and reopening condition.
- **Blast radius** — affected surfaces, contracts, data, users, and downstream propagation.
- **Reversibility** — `straightforward / costly / irreversible`, with the recovery path.
- **Confidence** — `high / medium / low`, attached to a specific claim, never substituted for
  evidence.

## Evidence and repository state

**Evidence** is inspectable support for a claim; **verification** is activity that produces it. A
**baseline** records the reference state or result a comparison depends on. **Claim provenance** is
`engineer stated / source cited:<reference> / inferred`. **Repository-state provenance** identifies
the exact state supporting evidence — prefer a commit SHA; delegated checkpoints make one normally
available. A **finding** is an atomic falsifiable claim with evidence, affected surface,
consequence, and confidence. A **review** is evidence-based inspection without required
independence; an **audit** is independent, adversarial, and scoped. Automated testing and
evaluation use the case / suite / run identity defined in the assurance policy; a selection of
cases is never called a suite.

## Handoffs, checkpoints, integration, and landing

A **handoff** transfers scoped context, state, or responsibility, and when persisted is the
artifact carrying that transfer; it references canonical state instead of copying it. Program
dispatch is its Program specialization. Session resumption needs no dedicated artifact: native
session continuity plus the canonical plans are the resumption contract, and a handoff is persisted
only when responsibility transfers with material context that cannot live in canonical state.

A **checkpoint** is verified recoverable state — durable plan state, repository-state provenance,
and verification evidence — normally represented by a delegated local commit under the Git policy.
**Integration reconciliation** follows any internal integration: verify the combined state and
update affected plan state, acceptance, baselines, and ledgers; it creates no parallel log.
**Landing** is engineer-triggered incorporation of one complete Task or whole Program into `main`
as one commit, per the Git policy. Completion, landing, shipping, and deployment remain distinct.

## External engine boundary

The external engine (currently Codex) participates only through self-contained file contracts:

- **Auditor** — consumes `audit-NN-dispatch.md`, produces `audit-NN.md` through the guarded
  launcher under the assurance policy. Challenge and remediation stay internal on the top model.
- **Ideation** — runs on portable registry prompts; syntheses enter as `inputs/` with cited
  provenance.

External contracts are self-contained by construction — executable without installed Protocol
instructions — and the external engine never reads its instructions from, or writes anything into,
canonical Protocol state. No other self-contained projection is maintained; context loss inside the
Claude-native line is handled by native session continuity and canonical artifacts.
