# Changelog

## 3.1.0 — 2026-08-13

Plan-maintenance skills: proof-then-judgment, engineer-confirmed.

- New `plan-sweep` skill — the engineer-triggered retirement sweep. A deterministic checker
  (`scripts/sweep-check.cs`) proves terminal + lifted + landed per `plan/` entry and doubles as
  the plan-tree survey; the skill adds the one judgment step (citation liveness) and deletes only
  engineer-confirmed directories, as one sweep commit. Legacy pre-3.0 plans fail the lift proof
  by design — their lift record is the register — and require explicit per-entry override.
- New `deferred-review` skill — register drift hunting: verifies every parked row against current
  repository state (closed by later work, fired triggers, claims that no longer reproduce, stale
  defaults) and applies only engineer-ratified transitions; a fired trigger proposes activation,
  never silent closure.
- Test baseline: new sweep-check suite (8 checks), wired into CI as the fifth suite.

## 3.0.0 — 2026-08-13

Three protocol-fit remediations from the first weeks of daily use. Major bump per the 2.0.0
precedent: the plan dialect gets stricter (terminal-lift arrows), so templates and validator must
propagate together.

- **Mini-protocol family prefix, catalogue re-cut by subject.** The ideation mini protocols are
  now `p-protocol-ideate-feature` (merges the former intent-shaping and repository ideation
  prompts — repository-grounded work shaping that ends in blast-radius and profile orientation),
  `p-protocol-ideate-concept` (new: pure conceptual ideation — approaches, abstractions, no
  repository required), and `p-protocol-ideate-domain` (was DDD ideation). The skills-only
  reservation of `p-protocol-*` is retired — skills are invoked by name, so no snippet prefix can
  collide with them — and the registry validator enforces family coherence instead
  (`p-protocol-*` prefix ↔ "mini protocol" description). The audit and utility prompts keep `p-*`.
- **New `ideation-audit` skill.** From inside an ideation mini protocol, dispatches fresh-context
  adversarial subagent audits of the session's settled conclusions — grounded (with repository
  access) and/or conceptual (deliberately without) — so design concepts and logic are settled
  before planning. Creates no Protodog state; findings reach planning only as `inputs/` with
  cited provenance.
- **Plan lifecycle: membership, lift, retirement.** `plan/` holds only plan-id directories and
  the deferred register (`plan/deferred.md`, template `templates/deferred-register.md`, validated
  on write via the hook). A plan may not go terminal as the sole carrier of a live obligation:
  deferred issues and accepted gaps lift to the register as self-sufficient rows or record their
  closure via disposition arrows — `→ D-NN` / `→ closed: <reason>`, tracks may also point at
  `→ ISSUE-NN` (enforced). Terminal, landed plan directories are deleted by the engineer-triggered
  retirement sweep; Git history on `main` is the archive. Stricter than 2.0.x in one case: a
  terminal plan written with un-arrowed deferred items is now rejected; live plans are unaffected
  until they complete.
- Test baseline: plan-validator 48 → 61, registry 7 → 9, hooks 35 → 38, launcher 8 unchanged.

## 2.0.0 — 2026-08-12

Plan artifact dialect change — breaking for existing plans, which must be migrated. Major bump so
`claude plugins update` propagates the templates and validator together; a plan written under 1.0.0
is rejected by the 2.0.0 validator.

- Every plan opens with `## Contents`, listing all other sections in document order; validated
  against the sections actually present.
- Rows carrying state — step, block, track, gate, acceptance criterion — put status marker,
  identifier, and status label in one identifier cell (`⬜ STEP-01 · ready`); the separate `Status`
  column is removed from the Steps, Tracks, Blocks, and Acceptance tables.
- Five status markers, reused across every row kind: ⬜ not started, 🟡 active, ✅ done, 🔴 needs
  attention, ⬛ closed without being met. Marker and label must agree (enforced). All are single
  codepoints — a marker requiring U+FE0F is invisible in an editor and lost on copy-paste. Spec
  acceptance-criteria tables are unaffected (they carry no status).
- Gates are a specified table with `open`/`settled` status; a settled gate strikes its identifier
  (`✅ ~~GATE-01~~ · settled`). Strikethrough is reserved for that and rejected elsewhere.
- Under `continuous` cadence, the owning plan is updated and verified as part of every work-row
  transition — prose rule, since no validator can observe a transition.
- Test baseline: plan-validator checks 16 → 48.

## 1.0.0 — 2026-08-03

Initial distribution release, extracted from the prototype workspace
(`/Users/pablo/source/tmp/protocol`, commit `9ec6704`; frozen archive of the vendor-neutral
release candidate, AUDIT-01 cycle, and build history).

- Protocol renamed **Protodog**; plugin `protodog`, marketplace `kennel`, skills `/protodog:task`
  and `/protodog:program`.
- Contents: normative core (5 docs), artifact templates (5), .NET file-based validators,
  enforcement hooks, and guarded Codex audit launcher (pinned `Configuration` record: executable,
  model, sandbox, timeout), portable prompt registries (utility + verbatim Boxer) under the
  registry style contract. Only the test drivers are bash.
- Test baseline: 16 plan-validator / 7 registry / 8 launcher / 35 hook checks (immutability tests
  are fixture-scaffolded; no archive state required).
