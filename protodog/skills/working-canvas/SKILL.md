---
name: working-canvas
description: Maintain the working canvas — a session-scoped visual model of diagram-suited work (a mapping between structures, a topology, a lifecycle, or a data flow) — from the baseline turn onward, through ideation, planning, and execution. Use when such a session begins, when a decision surface or gate needs its options drawn in context, or when progress recoloring is due.
---

# Working canvas

You are maintaining the working canvas: the session's visual model of the problem under work,
seeded at baseline and kept current forward — not a companion drawn at close. This is
Claude-native mechanics for the canvas doctrine stated in the ideation mini protocols
(`p-protocol-ideate-*`) and the Task/Program profile prose; it is not a Protocol entry point and
creates no Protodog state. The canvas is a working view under the Foundation's rule: state is
mirrored *to* it, never *from* it — the canvas illustrates; the session ledger, the plan, or the
conclusions document rules. Never let a diagram's existence argue against reopening a settled
conclusion: redraw on reopening is the accepted cost, and a discarded canvas is cheap.

## Detection

Canvas on when the subject is a mapping between two structures, a topology, a lifecycle, or a
data flow; off for pure naming, policy, or logic sessions. When cues are mixed, default on — the
cost asymmetry favors a canvas that gets discarded over prose that needed pictures.

## Home and rendering

One session-scoped markdown file with mermaid diagrams (default `canvas.md` at the repository
root, untracked). When seeding it, suggest the engineer keep it open in an IDE with mermaid
preview — terminal chat does not render mermaid; an open preview does. It never enters checkpoint
commits or canonical artifacts. Canvas rules are structural, not stylistic: where the host
repository carries a `mermaid-style` skill, defer styling to it.

## Structure

- **One master canvas**, seeded once at baseline, at fixed altitude: lanes are the separated
  concerns, roughly ten boxes — a number loosely held — never zooming into mechanism. The
  **landing rule** makes the separation load-bearing: every element the session produces lands in
  exactly one lane; an element that cannot is a design finding — a missing concern — not a
  drawing problem, and is surfaced as such.
- **Zoom diagrams** one level down, one per open decision surface, each owning exactly one
  question. A zoom that accumulates a second question splits.
- **Alternatives are drawn, not listed**: each materially distinct option is sketched in the
  affected region plus one ring of surrounding context — the ring anchors it in the overall
  process — before the prose comparison. On settlement the winner is drawn into its zoom and the
  losing sketches are deleted; the conclusions record keeps *why* they lost.

## Legend, fixed across sessions

Every canvas reads identically: **gray** = exists today, **blue** = under discussion (open
surface), **green** = settled, **yellow** = gated / parked, **red** = reopened, **purple** =
built / verified (execution only, distinct from settled-direction).

## Stations

One doctrine with per-station hooks — the structure rules and legend hold everywhere; only the
base canvas changes:

| Station | Base canvas | Hook |
|---|---|---|
| Ideation | The problem model | Seed at baseline; zoom per decision surface; settlement, correction, and `ideation-audit` reconciliation recolor (reopened = red) |
| Planning | The plan's structure — tracks, steps, blocking order | The gate/dependency order rendered as a DAG, states colored open / leaning / ruled / deferred |
| Execution (Task, Program) | The system as built so far | Gate-presentation contract per the profile prose: a gate's options drawn in the affected region plus one ring; the ruling recolors, and step completion recolors built/verified — structure changes are rare post-planning, so maintenance collapses to recoloring, which doubles as the engineer's progress view |

Program-scale work — many gates, long horizon — is where the rendered gate DAG pays most.

## Continuity and close

Continuity across stations is per-station seeding through the existing promotion rule, not one
evolving file: an ideation canvas promoted under `inputs/` on explicit instruction seeds the
planning canvas; the planning canvas seeds execution. At close the canvas *is* the diagram
companion, minus a polish pass. Ephemeral by default at every station — promote only on explicit
instruction; an execution canvas retires with its plan unless the engineer explicitly graduates
it into repository documentation.
