# Changelog

## 3.5.0 — 2026-08-15

Session retrospectives: the practice that produced the working canvas, codified.

- New `p-protocol-retro` mini protocol — a retrospective run at the tail of a working session,
  examining how the work went rather than what it concluded: items that iterated beyond their
  substance (and what form would have settled them sooner), uncertainties that dragged along,
  recurring misreads revealed by engineer corrections, protocol-fit friction versus session-local
  accident, and the practices worth keeping. Observations cite concrete session moments with
  engineer-stated versus inferred provenance; corrected misdiagnoses are recorded so they are not
  re-proposed; remediations leave as handoffs with open points — not implementation plans —
  naming where each belongs (session habit, method change, or a later amendment in the protocol
  source). Exemplar: `inputs/ideation-visual-support-handoff.md`, the retro product that became
  3.2.0.
- README drift audit (engineer-dispatched), all findings adopted: the release-ritual sentence now
  matches practice (engineer-reviewed release commits; Protodog plans/specs for heavier changes);
  gates described as the HIL core rather than the only HIL interface; the install line covers all
  shipped skills; the profile flowchart binds specs at readiness (after planning), opens planning
  with the assurance interrogation, and shows the canvas, retro, and pre-entry orientation; the
  denial-log note documents the `PROTOCOL_DENIAL_LOG` override; the hook-latency figure is
  dropped (engineer-ruled), keeping the native-binary tip.
- Rationale recorded as `docs/design-rationale.md` decision 17. Test baseline unchanged.

## 3.4.0 — 2026-08-15

Profile recommendation on request; the `p-protocol-*` family re-cut to carry it. Rationale
recorded as `docs/design-rationale.md` decision 16.

- New `p-protocol-recommend-profile` registry snippet — a one-shot grounded Task-vs-Program
  orientation for cold entry (work that skips ideation's profile orientation): weighs bounded
  objective versus long-lived horizon, latent track topology, cross-track state / dispatch /
  concurrent-writer needs, and proportionality (decomposability alone is not a Program signal);
  returns the recommendation with confidence, driving criteria with evidence, flip conditions,
  and the suggested invocation line. Advisory only, and the Foundation now says so: a pre-entry
  recommendation is never solicited by an entry skill and never a selection — the engineer
  selects by invocation.
- Namespace re-cut (second re-ruling of `p-protocol-*`): the prefix marks the
  protocol-affiliated family; the kind is declared in the description — "mini protocol"
  (governed multi-turn session) or "protocol utility" (one-shot) — with prefix ↔ declared-kind
  coherence enforced by the registry validator. Style contract updated accordingly.
- Test baseline: registry 9 → 11.

## 3.3.0 — 2026-08-15

The assurance interrogation: audit and paid-evaluation scheduling is settled state at planning
entry, not lingering uncertainty. Engineer-reported pain point; rationale recorded as
`docs/design-rationale.md` decision 15.

- The assurance policy defines the interrogation: Task planning, and each track's planning in a
  Program (Program-wide scheduling once, at Program planning), opens by interrogating the
  scheduling of the authorization-gated instruments — audits and paid or external evaluation
  runs — and persists exactly one outcome per instrument in the owning plan before execution
  readiness: selected with declared placement or run bounds; a decision gate at planning entry
  when indicators support an instrument no selecting authority has ruled on (never silently
  selected, never silently dropped); or the explicit not-scheduled decision with its basis.
- Task and Program readiness definitions and both profile assurance sections carry the hook; the
  entry skills open planning with the interrogation, and an omitted `assurance:` argument no
  longer means a silent no-assurance default — it means the interrogation settles it. Launch
  authorization gates are unchanged; a later change reopens the outcome as an ordinary material
  delta.
- Prose-level rule: no artifact-dialect, template, or enforcement change. Test baseline
  unchanged.

## 3.2.0 — 2026-08-15

Working canvas: visual support for diagram-suited work, one doctrine across stations. Provenance:
`inputs/ideation-visual-support-handoff.md` (relocated from the originating session repository);
open-point rulings recorded as `docs/design-rationale.md` decision 14.

- **Portable clause in the ideation mini protocols.** All three `p-protocol-ideate-*` snippets:
  a diagram-suited subject (a mapping between structures, a topology, a lifecycle, a data flow)
  maintains a working canvas from the baseline turn — one master model at fixed altitude with
  concern lanes and the landing rule, one-question zooms per decision surface, alternatives drawn
  in the affected region plus one ring of context before the prose comparison, winners drawn in
  and losing sketches deleted on settlement, a diagram's existence never arguing against
  reopening. Host-neutral: structured prose where the host renders no diagrams.
- **New `working-canvas` skill** — the Claude-native mechanics: detection cues (mixed cues
  default on), the fixed cross-session legend (gray exists / blue open / green settled / yellow
  gated / red reopened / purple built-verified), the session-scoped mermaid file with the
  IDE-preview hint, styling deference to a host `mermaid-style` skill, per-station base canvases
  (problem model / plan structure with the gate DAG / system as built), per-station seeding
  through the existing promotion rule, promotion only on explicit instruction.
- **`ideation-audit` coupling**: reconciliation recolors reopened conclusions red; the brief
  stays self-contained — a canvas informs its distillation, never substitutes for it.
- **Gate presentation in the profiles.** Task and Program prose gain the contract: where a canvas
  is maintained, a gate surfaced for ruling includes its options drawn in the affected region
  plus one ring of context; the ruling recolors, execution recolors built work, and the plan
  remains the sole authority. The Foundation lists the canvas among working views and the skill
  in the normative set.
- Test baseline: unchanged (61 plan-validator / 9 registry / 38 hooks / 8 launcher / 8
  sweep-check) — no artifact-dialect or enforcement change; registries re-validated.

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
