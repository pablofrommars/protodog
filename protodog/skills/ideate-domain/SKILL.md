---
name: ideate-domain
description: Open a multi-turn, DDD-informed domain ideation mini protocol converging on a pragmatic model for a stated problem. Engineer-invoked with the domain problem as argument.
disable-model-invocation: true
argument-hint: [problem or process, domain terms, references, rules, constraints]
---

# Domain ideation mini protocol

You are opening the domain ideation mini protocol. It is a governed working session outside the
Protocol: it creates no Protodog state, binds nothing, and never implies conformance. Inside this
session `/protodog:ideation-audit` applies adversarial pressure to settled conclusions and
`/protodog:working-canvas` keeps the visual model, which domain work almost always warrants.

## Arguments

The invocation text is the context block below, in free text. On Claude Code it replaces `$ARGUMENTS`;
on Codex it follows the skill mention and is read as the same block. The five
elements the session relies on are listed under the text; an element the text does not supply is
treated as unknown and raised when a decision needs it, never demanded upfront.

<task>
Collaboratively explore the domain and converge on a pragmatic model useful for the stated problem.
</task>

<context>
$ARGUMENTS

Elements read from the text above: the problem or process; domain language and exact meanings; relevant types, processes, documentation, or tests; known business rules and invariants; constraints, disagreements, and uncertainty.
</context>

<instructions>
- Treat this as a multi-turn modeling session. Do not attempt a complete domain model in the first response; establish the first useful language, rule, boundary, or invariant surface and let my feedback shape subsequent turns.
- Ground proposed language and rules in supplied domain evidence; distinguish facts, assumptions, alternatives, and questions for a domain expert.
- Use supplied domain names exactly. Surface overloaded, conflicting, or unstable terminology; never silently normalize it or substitute a convenient synonym.
- Use DDD technical terms precisely. Do not label something an entity, value object, aggregate, bounded context, or domain event unless its defining characteristics are supported.
- When repository references are supplied, inspect the relevant types and processes as evidence of the current implementation and model. Do not treat existing code as automatic domain truth or authority for future design.
- Use strategic DDD concepts only where boundaries, ownership, or relationships materially affect the problem. Use tactical concepts only where behavior and invariants justify them.
- Challenge leaky boundaries, anemic models, accidental coupling, and pattern-shaped designs that lack domain evidence.
- Do not force bounded contexts, aggregates, value objects, events, repositories, services, or factories into the result. Explicitly recommend CRUD or a simpler model when sufficient.
- Advance one coherent domain decision surface or small related batch per turn, reconcile my corrections, and continue until I explicitly close the session.
- Maintain a lightweight evolving domain-language ledger as local session state. Surface it only when terminology changes materially or I ask.
- When the subject is diagram-suited — a mapping between structures, a topology, a lifecycle, or a data flow — maintain a working canvas beside the prose from the baseline turn: one master model at fixed altitude whose lanes are the separated concerns, roughly ten boxes, never mechanism, plus zoom views one level down that own exactly one question each, opened when their decision surface opens and split when a second question accumulates. Every element the session produces lands in exactly one lane; an element that cannot is a design finding — a missing concern — not a drawing problem. Render as diagrams where the host supports them, as structured prose otherwise; when detection cues are mixed, default to keeping the canvas — a discarded canvas is cheap.
- Sketch materially distinct alternatives on the canvas before comparing them in prose, each drawn in the affected region plus one ring of surrounding context. On settlement draw the winner into its zoom and delete the losing sketches — the session record keeps why they lost — and reconcile the canvas with every correction or reopened conclusion; never let a diagram's existence argue against reopening. The canvas follows the session's persistence rule: ephemeral by default, promoted with the results on explicit instruction; at close it is the visual companion of the synthesis.
- When I ask to be interrogated, or a batch of open choices has accumulated, present the reassessment first, then put the choices through the host's structured question tool with the recommendation marked and the default stated (`/protodog:interrogate` carries that contract).
- Do not implement, perform an audit, or create an artifact unless I request it.
</instructions>

<output>
Work conversationally. On explicit close, return a concise in-session summary of the settled model, rationale, remaining domain questions, and next validation. Create no file by default.
</output>
