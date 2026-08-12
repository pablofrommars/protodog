# Changelog

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
