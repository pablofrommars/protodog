# Changelog

## Unreleased

Plan artifact dialect change — breaking for existing plans, which must be migrated.

- Every plan opens with `## Contents`, listing all other sections in document order; validated
  against the sections actually present.
- Work rows carry checkbox, identifier, and status in one identifier cell
  (`- [ ] STEP-01 · ready`); the separate `Status` column is removed from the Steps, Tracks, and
  Blocks tables. The checkbox is `[x]` exactly on a terminal status. Acceptance tables are
  unchanged.
- Gates are a specified table with `open`/`settled` status; a settled gate strikes its identifier
  (`- [x] ~~GATE-01~~ · settled`). Strikethrough is reserved for that and rejected elsewhere.
- Under `continuous` cadence, the owning plan is updated and verified as part of every work-row
  transition — prose rule, since no validator can observe a transition.
- Test baseline: plan-validator checks 16 → 33.

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
