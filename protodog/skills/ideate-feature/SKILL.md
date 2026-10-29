---
name: ideate-feature
description: Open a multi-turn, repository-grounded feature ideation mini protocol that settles direction, alternatives, and sizing before any planning. Engineer-invoked with the intent as argument.
disable-model-invocation: true
argument-hint: [intent, surfaces, constraints, favored direction]
---

# Feature ideation mini protocol

You are opening the feature ideation mini protocol. It is a governed working session outside the
Protocol: it creates no Protodog state, binds nothing, and never implies conformance. Protocol
entry remains `/protodog:task`, invoked by the engineer when sizing is settled. Inside this
session `/protodog:ideation-audit` applies adversarial pressure to settled conclusions,
`/protodog:working-canvas` keeps the visual model where the subject is diagram-suited, and
`/protodog:runbook` cuts the session into Tasks when it closes.

## Arguments

The invocation text is the context block below. On Claude Code it replaces `$ARGUMENTS`; on
Codex it follows the skill mention and is read as the same block. Nothing is asked for upfront:
a missing element is raised when the session reaches the decision that needs it.

<task>
Collaboratively shape the supplied intent into a defensible feature direction: explore the relevant repository surfaces, widen and test meaningful alternatives, and settle conclusions — including, when execution is in sight, enough orientation to ballpark blast radius, cost, and Task fit — without planning or implementing.
</task>

<context>
$ARGUMENTS

Expected here: the intent or problem, relevant repository surfaces and references, known scope and exclusions, constraints, and any direction already favored.
</context>

<instructions>
- Treat this as a multi-turn working session. Do not attempt to complete the exploration or produce a final guide in the first response.
- Ground material claims in applicable repository instructions, documentation, code, and immediate seams. Distinguish current behavior, documented intent, my direction, inference, and uncertainty.
- Begin with a concise preliminary baseline: relevant behavior and boundaries, forces behind the current shape, concrete friction, contradictions, and material unknowns. Let my feedback correct or redirect it as part of the conversation rather than as a formal approval gate.
- In each turn, advance one coherent decision surface or small related batch. Generate materially distinct alternatives only where the direction remains open; test consequential assumptions, strongest objections, trade-offs, reversibility, and affected seams before recommending.
- Maintain local session state as `settled conclusions / active directions / unresolved decisions`. Reconcile it after every correction or new repository fact. Surface it when materially changed or requested; do not print a ceremonial ledger every turn.
- When the subject is diagram-suited — a mapping between structures, a topology, a lifecycle, or a data flow — maintain a working canvas beside the prose from the baseline turn: one master model at fixed altitude whose lanes are the separated concerns, roughly ten boxes, never mechanism, plus zoom views one level down that own exactly one question each, opened when their decision surface opens and split when a second question accumulates. Every element the session produces lands in exactly one lane; an element that cannot is a design finding — a missing concern — not a drawing problem. Render as diagrams where the host supports them, as structured prose otherwise; when detection cues are mixed, default to keeping the canvas — a discarded canvas is cheap.
- Sketch materially distinct alternatives on the canvas before comparing them in prose, each drawn in the affected region plus one ring of surrounding context. On settlement draw the winner into its zoom and delete the losing sketches — the session record keeps why they lost — and reconcile the canvas with every correction or reopened conclusion; never let a diagram's existence argue against reopening. The canvas follows the session's persistence rule: ephemeral by default, promoted with the results on explicit instruction; at close it is the visual companion of the synthesis.
- When I ask to be interrogated, or a batch of open choices has accumulated, present the reassessment first, then put the choices through the host's structured question tool with the recommendation marked and the default stated (`/protodog:interrogate` carries that contract).
- Continue automatically to the next useful decision surface after I settle the current one unless I redirect the session. Do not infer that agreement closes the session.
- Sizing is a conclusion, not an entry: state blast radius, cost, and Task fit as settled conclusions when execution is in sight, and leave the invocation to me. Do not create Protodog gates, plans, specs, or implementation.
- Close only on my explicit instruction. Do not persist by default; results destined for planning are saved under inputs/ with cited provenance.
</instructions>

<output>
Work conversationally across turns. On explicit close, return an in-session synthesis of settled conclusions, necessary rationale, rejected directions worth preserving, unresolved decisions and assumptions, and useful next validation. If I explicitly request persistence, write the requested artifact beginning with `Why this document exists`; preserve conclusions and continuation context rather than session narrative.
</output>
