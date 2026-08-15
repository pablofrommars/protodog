# Prompt Registry Style Contract

## Why this document exists

This is the single mechanical authoring contract for the distributed prompt registries of the
Claude-native line. It supersedes `bundle/prompt-registry-style.md`, rescoped: the Agentic
Execution Protocol's entry points are skills, not snippets, so no protocol snippet registry exists.
The registries carry host-portable prompt text — usable in any host, including the external
ideation engine — which is precisely why they remain snippets rather than skills.

Status: committed registry mechanics, Claude-native line.

## Registries and families

| Registry | Content | Prefixes |
|---|---|---|
| `prompt-utility.code-snippets` | Protocol-family prompts (ideation mini protocols, protocol utilities), full prompt utilities, and composition helpers | `p-protocol-*`, `p-*`, `c-*`, `e-*`, `g-*` |
| `boxer.code-snippets` | Verbatim standalone Boxer family and its local dialect | `bx-*` |

The `p-protocol-*` prefix marks the protocol family: prompts affiliated with Protodog itself. The
family has two kinds, declared by the description and enforced coherent with the prefix by the
registry validator: a **mini protocol** ("mini protocol" in the description) opens a governed
multi-turn working session with its own session state and close semantics; a **protocol utility**
("protocol utility" in the description) is a one-shot prompt about the Protocol — advisory or
orienting, complete in one expansion. Either kind is a standalone bounded contract: it may align
with Foundation terminology, but it never implies Protodog conformance and never writes canonical
Protocol state — Protocol entry remains skills-only, and skills are invoked by name, so no snippet
prefix can collide with them. The former skills-only reservation of `p-protocol-*` was retired
2026-08-13 (it defended against a collision that cannot occur); the family was re-cut 2026-08-15
from mini-protocols-only to the protocol affiliation it now carries, with the kind moved into the
description.

## Snippet mechanics

Every file is valid VS Code JSON-with-comments with one root object. Every snippet has a unique
human-readable object key; `scope: "markdown"`; one unique, descriptive `prefix` across the
complete registry set; a `body` array of strings; and a concise factual `description` that states
what expanding it does and disambiguates mini protocols from ordinary utilities. Order entries by
family and expected use. Comments may mark family boundaries; they carry no semantic instructions.

Use shallow XML-like tags only where they separate concerns that materially help the reader:
`<task>`, `<context>`, `<instructions>`, `<guardrails>`, `<output>`; example helpers may use
`<examples>`/`<good_example>`/`<bad_example>`/`<reasoning>`; Boxer keeps its verbatim local
dialect. Prefer the shared tags over synonyms; omit empty optional sections; keep prompts readable
as plain Markdown when tags are removed.

Placeholders: number tab stops in fill order with concrete readable hints; choice placeholders only
for small closed sets; `$TM_SELECTED_TEXT` only when selection is a meaningful input; `$0` as the
final stop when composition is expected; clear path placeholders rather than silently selected
destinations; no snippet depends on another having been expanded first. Every full utility prompt
and mini protocol is self-contained for its declared operating environment.

## Validation

`protodog/validators/validate-registry.cs` deterministically checks parseability, required
fields, key and prefix uniqueness across the registry set, tab-stop ordering, and protocol-family
coherence (`p-protocol-*` prefix ↔ a declared kind — "mini protocol" or "protocol utility" — in
the description). Deterministic validation
does not replace the risk-driven `p-audit-registry` adversarial review.
