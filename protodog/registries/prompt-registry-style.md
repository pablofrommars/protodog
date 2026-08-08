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
| `prompt-utility.code-snippets` | Full prompt utilities, standalone mini protocols, and composition helpers | `p-*`, `c-*`, `e-*`, `g-*` |
| `boxer.code-snippets` | Verbatim standalone Boxer family and its local dialect | `bx-*` |

The `p-protocol-*` prefix is reserved for Protocol entry skills and may not be used by any snippet
(enforced by the registry validator). A utility or mini protocol must not imply Protocol
conformance or write canonical Protocol state; standalone mini protocols may align with Foundation
terminology but operate under their own bounded contracts.

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
fields, key and prefix uniqueness across the registry set, tab-stop ordering, and the
`p-protocol-*` reservation. Deterministic validation does not replace the risk-driven
`p-audit-registry` adversarial review.
