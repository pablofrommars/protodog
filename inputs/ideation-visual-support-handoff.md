# Protodog Ideation — Visual Support Handoff

*Relocated 2026-08-15 from the originating session repository (tsu worktree
`studies-deployment-model`, `inputs/protodog/`), executing §6 "Relocation". Content otherwise
verbatim.*

## Why this document exists

Handoff from the retro of the studies-deployment-model ideation session (2026-08-14/15, Pablo +
assistant, Protodog ideation protocol + `protodog:ideation-audit`). It records the pain points
that surfaced in that session and the mitigation design identified for baking into Protodog, so
the protocol amendment can be executed later — likely in the protodog source, where this file
should be relocated. It is a handoff, not an implementation plan: the mitigation is shaped, its
open points are listed, nothing is drafted in protocol idiom yet. Provenance: pain points and
the granularity principle are Pablo's statements; the mitigation mechanics are the session's
design, corrected once by Pablo (see P2).

## Contents

1. [Session context](#1-session-context)
2. [Pain points](#2-pain-points)
3. [What worked (the evidence base)](#3-what-worked-the-evidence-base)
4. [Mitigation: the working canvas](#4-mitigation-the-working-canvas)
5. [Lifecycle extension: gates in Task and Program](#5-lifecycle-extension-gates-in-task-and-program)
6. [Open points for the implementer](#6-open-points-for-the-implementer)

## 1. Session context

A feature-class ideation session: mapping study structure (derived task graphs) into deployment
units and the supporting cloud services of a run — an inherently diagram-suited problem class
(structure-to-structure mapping, topology, lifecycles, data flow). The session ran multi-turn
prose refinement over decision surfaces, persisted conclusions to
`inputs/deployment/studies-deployment-model.md`, ran the both-mode adversarial audit, and
produced a color-coded diagram companion (`studies-deployment-model-diagrams.md`) — but only at
session close, on explicit request.

## 2. Pain points

**P1 — the problem under refinement had no visual support while it was being refined** (the
primary pain; Pablo, visual learner). Alternatives were compared in prose columns; relations
between items lived only in sentences; mechanism explanations stayed textual until late.
Concrete session moments:

- **Alternatives without context**: the seam-medium comparison (Postgres cells vs blob vs Table
  storage vs payload-in-event) and the declaration-home comparison (registry vs code-table vs
  host-implicit) ran as multi-paragraph prose. What was missing: each alternative drawn *in the
  overall process* — where it sits, who reads it, where its specific failure occurs.
- **Relations without a picture**: the load-bearing insight that one cell write serves four
  roles (seam, freshness fingerprint, join barrier, tracking) was carried in sentences across
  several turns.
- **Mechanism without a picture**: queue visibility mechanics (retry = non-deletion, not
  re-insertion) were prose until near the end; Pablo: *"now I understand and makes sense why
  duplication was hard to understand."* One message-lifecycle diagram drawn when the duplicates
  surface opened would have pre-empted roughly four turns of confusion.
- **Placement confusion as a symptom**: *"are we back to the question where we should allow the
  cut to take place?"* — the duplicate-execution thread was the settled cut-site space under a
  second cost axis, which a maintained picture of that space would have shown on arrival.

**P2 — a corrected misdiagnosis, recorded so it is not re-proposed**: the first retro reading
located the pain in session-state placement ("where are we among the decision surfaces") and
proposed a rendered session ledger map. Pablo redirected: the issue was **visual support on the
problem content**, not on session state. The mitigation below is content diagrams; a
session-map mechanism is explicitly *not* the fix for this pain and was shelved.

## 3. What worked (the evidence base)

- The **closing diagram companion** was requested by Pablo and immediately valued — proof the
  visual form fits; the failure was timing (close, not during).
- The **one-question-per-diagram** discipline and the **fixed color legend** (established vs
  gated) made the companion legible; both should carry into the mitigation.
- Pablo's standing habit: a **scratchpad file kept open in the IDE** all session — the natural
  render surface (terminal chat does not render mermaid; an IDE file with mermaid preview does).
- The stated granularity principle, Pablo's words: *"the trick is finding the right granularity —
  big picture with clear separation of concerns."*

## 4. Mitigation: the working canvas

The closing companion, started at baseline and maintained forward — a working canvas for the
problem, not a closing artifact.

**Structure — the granularity principle as rules:**

- **One master canvas**, drawn once after the baseline turn, at fixed altitude: lanes are
  *concerns* (this session's would have been front door / unit fabric / stores /
  observability-failure), roughly ten boxes, never zooming into mechanism. The
  **landing rule** makes separation of concerns load-bearing: every element the session
  produces must land in exactly one lane; an element that cannot is a design finding (a missing
  concern), not a drawing problem.
- **Zoom diagrams** one level down, created when a decision surface opens, each owning
  **exactly one question**. A zoom that accumulates a second question splits.

**Alternatives are drawn in context, not listed**: each materially distinct option gets a mini
diagram of the **affected region plus one ring of context** — the ring anchors it in the
overall process. On settlement the winner is drawn into its zoom and the losing sketches are
deleted; the conclusions ledger keeps *why* they lost (maintaining rejected diagrams is churn).

**Lifecycle** (piggybacking on cadences the protocol already has — no new ceremony):

| Moment | Canvas action |
|---|---|
| Baseline, diagram-suited class detected | Seed the master |
| Decision surface opens | Create its zoom; sketch alternatives in context, before the prose comparison |
| Settlement / correction / audit reconciliation | Draw the winner in; recolor states; drop losers |
| Session close | The canvas *is* the companion, minus a polish pass; promote under `inputs/` on instruction, else discard |

**Detection cues** for "diagram-suited class" (canvas on): the subject is a mapping between two
structures, a topology, a lifecycle, or a data flow. Canvas off for pure naming / policy /
logic sessions.

**Home and persistence**: one session-scoped markdown file with mermaid, the engineer keeps it
open in the IDE with preview (formalizing the scratchpad habit). Ephemeral by default; promoted
on explicit instruction — consistent with the protocol's persistence rule.

**Fixed legend across sessions** (so every canvas reads identically): gray = exists today,
blue = under discussion (open surface), green = settled, yellow = gated/parked, red = reopened.

**Where each half lives**, respecting Protodog's architecture:

- **Portable clause** in the `p-protocol-ideate-*` prose: the contract — diagram-suited classes
  maintain a working model (master at fixed altitude, one-question zooms, alternatives in
  context with one ring, settlement accretes, close promotes). Host-neutral; prose fallback
  where no diagram tooling exists.
- **Claude-native plugin skill** (beside `ideation-audit`): the mechanics — detection cues, the
  legend, deference to the `mermaid-style` skill for styling (canvas rules are structural, not
  stylistic), the IDE-preview hint, promotion at close.
- **Coupling to `ideation-audit`**: reconciliation recolors canvas nodes (reopened = red). The
  audit brief must remain self-contained per that skill's contract — the canvas informs the
  brief's distillation but is not a substitute for it.

## 5. Lifecycle extension: gates in Task and Program

Foreseen by Pablo at retro close: the same pain class recurs post-planning and during
implementation, wherever **gates need settlement** mid-execution. The evidence already exists in
the host repository: `plan/infra-cloud/deployment-forks.md` was hand-commissioned as a diagram
companion precisely because G1's fork corpus needed visual settlement support ("it illustrates,
this document rules"), and `planning-context.md` §10 — open questions ordered by blocking — is a
dependency DAG written as a numbered prose list.

The generalization is **one doctrine with per-station hooks, not three features**. The invariant
rules (alternatives in context with one ring, one question per diagram, fixed legend) hold at
every station; only the *base canvas* changes:

| Station | Base canvas | Hook |
|---|---|---|
| Ideation | The problem model | §4 as designed |
| Planning | The plan's structure — tracks, steps, blocking order | The gate/dependency ledger rendered as a DAG, states colored open / leaning / ruled / deferred |
| Execution (Task, Program) | The system as built so far | **Gate-settlement presentation contract**: a gate surfaced for ruling in a diagram-suited domain includes its options drawn in the affected region + one ring; the ruling recolors |

**Canvas continuity**: the ideation companion does not retire at planning — it is the planning
input's base layer, and during execution the legend gains one state (**built/verified**,
distinct from settled-direction) that the executor recolors at step completion. Structure
changes are rare post-planning, so execution-time maintenance collapses to recoloring — which
doubles as the engineer's progress view at no extra cost.

Profile-side homes: the gate-presentation contract belongs in the Task/Program profile prose
(the analog of the ideation clause); the ledger rendering and recolor mechanics belong in the
corresponding plugin skills. Program-scale work (many gates, long horizon) is where the
rendered gate DAG pays most.

## 6. Open points for the implementer

- **Rejected-sketch policy**: delete-on-settlement (leaned, anti-churn) vs a small rejected
  gallery. The conclusions document's rejected-directions table already preserves the *why*.
- **Master altitude bound**: a number (~10 boxes) vs judgment. The session leaned "a number,
  loosely held."
- **Ambiguous problem class**: default canvas-on or canvas-off when detection cues are mixed.
  Unresolved; the cost asymmetry (a discarded canvas is cheap) suggests on.
- **Churn tolerance**: draw-at-settlement risks redraw when audits reopen conclusions (it
  happened this session). The rule adopted in-session: draw on settlement, treat as cheap to
  discard, never let a diagram's existence argue against reopening — worth stating in the
  clause verbatim.
- **Relocation**: this file belongs in the protodog source alongside the mini protocols; it was
  written here because the session ran here.
- **Execution-station additions** (§5): whether canvas continuity across stations is one
  evolving file or per-station files with promotion; whether a landed system's final canvas
  graduates into `docs/` (which has its own conclusions-only doctrine) or retires with the
  plan; the exact color for the built/verified state (the shared palette has headroom:
  indigo/purple/pink unused by the current legend).
