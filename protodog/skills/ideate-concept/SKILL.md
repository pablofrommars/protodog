---
name: ideate-concept
description: Open a multi-turn conceptual ideation mini protocol for abstractions and approaches, independent of any repository. Engineer-invoked with the concept as argument.
disable-model-invocation: true
argument-hint: [concept, motivating problem, sources, position held]
---

# Concept ideation mini protocol

You are opening the concept ideation mini protocol. It is a governed working session outside the
Protocol: it creates no Protodog state, binds nothing, and never implies conformance. Inside this
session `/protodog:ideation-audit` in `conceptual` mode applies adversarial pressure to settled
conclusions and `/protodog:working-canvas` keeps the visual model where the subject is
diagram-suited.

## Arguments

The invocation text is the context block below. On Claude Code it replaces `$ARGUMENTS`; on
Codex it follows the skill mention and is read as the same block. Nothing is asked for upfront:
a missing element is raised when the session reaches the question that needs it.

<task>
Collaboratively explore the supplied concept and converge on defensible conceptual conclusions — abstractions, approaches, and their limits — independent of any repository or execution intent.
</task>

<context>
$ARGUMENTS

Expected here: the concept or question, the motivating problem, known prior art or sources, constraints, and any position already held.
</context>

<instructions>
- Treat this as a multi-turn conceptual working session. No repository grounding is assumed or required; when sources, papers, or artifacts are supplied, cite them exactly.
- Distinguish established knowledge, supported inference, working hypothesis, and speculation. Never present speculation as settled; state what evidence, experiment, or worked example would settle a material unknown.
- Begin with a concise framing baseline: the core question, the forces on it, materially distinct schools or approaches, and material unknowns. Let my feedback correct or redirect it as part of the conversation rather than as a formal approval gate.
- In each turn, advance one coherent conceptual surface or small related batch. Stress abstractions against concrete cases, edge conditions, and failure modes; test the strongest objections and materially distinct alternatives before recommending.
- Maintain local session state as `settled conclusions / active directions / unresolved questions`. Reconcile it after every correction or new fact. Surface it when materially changed or requested; do not print a ceremonial ledger every turn.
- When the subject is diagram-suited — a mapping between structures, a topology, a lifecycle, or a data flow — maintain a working canvas beside the prose from the baseline turn: one master model at fixed altitude whose lanes are the separated concerns, roughly ten boxes, never mechanism, plus zoom views one level down that own exactly one question each, opened when their decision surface opens and split when a second question accumulates. Every element the session produces lands in exactly one lane; an element that cannot is a design finding — a missing concern — not a drawing problem. Render as diagrams where the host supports them, as structured prose otherwise; when detection cues are mixed, default to keeping the canvas — a discarded canvas is cheap.
- Sketch materially distinct alternatives on the canvas before comparing them in prose, each drawn in the affected region plus one ring of surrounding context. On settlement draw the winner into its zoom and delete the losing sketches — the session record keeps why they lost — and reconcile the canvas with every correction or reopened conclusion; never let a diagram's existence argue against reopening. The canvas follows the session's persistence rule: ephemeral by default, promoted with the results on explicit instruction; at close it is the visual companion of the synthesis.
- When I ask to be interrogated, or a batch of open choices has accumulated, present the reassessment first, then put the choices through the host's structured question tool with the recommendation marked and the default stated (`/protodog:interrogate` carries that contract).
- Continue automatically to the next useful surface after I settle the current one unless I redirect the session. Do not infer that agreement closes the session.
- Do not create Protodog state, plan, or implement.
- Close only on my explicit instruction. Do not persist by default; results destined for planning are saved under inputs/ with cited provenance.
</instructions>

<output>
Work conversationally across turns. On explicit close, return an in-session synthesis of settled conclusions, necessary rationale, rejected directions worth preserving, unresolved questions, and useful next validation. If I explicitly request persistence, write the requested artifact beginning with `Why this document exists`; preserve conclusions and continuation context rather than session narrative.
</output>
