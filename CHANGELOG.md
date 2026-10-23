# Changelog

## 4.0.1 — 2026-10-23

- `p-protocol-runbook` can cut the visible session directly, retaining settled decisions,
  corrections, source provenance, and supplied host/model preferences. Explicit source documents
  remain supported; repository HEAD is inspected, and an already settled landing policy is reused.
- Prerequisites distinguish separately landed Tasks on `main` from a declared line's branch.
  Dispatch prompts carry enough context for a fresh session; assurance recommendations become
  invocation overrides only when selected. The Codex invocation's dollar sign survives snippet
  expansion. Runbooks remain advisory and are saved only on explicit instruction.

## 4.0.0 — 2026-10-22

- Program is retired. Task is the only execution profile: the Program protocol, its entry skill,
  the program, track, and dispatch templates, their validation and position lines, the issue
  ledger lift path, the profile-selection ceremony and its recommendation snippet are gone, and
  both hosts expose the same package. A sequence of Tasks under a runbook is the multi-unit
  shape; the Git policy admits a declared line of Tasks as one commit unit. Design rationale
  decisions 27 to 29 carry the reasons.
- The deferred review proposes clusters beside its five transitions: for a row being picked up,
  the parked rows sharing its code surface or trigger and its verification, one reason line per
  member and per excluded row, ratified as one request and adopted by the next Task as steps.
  The review lifts no rows of its own, and runs in any mode at the landing moment, after the
  rebase onto main and before the commit to main; Task completion names that moment instead of
  offering a review against a degraded closing context.
- `scripts/worktree-survey.cs` reports worktree retirement candidates: merged (ancestor,
  patch-equivalent, or squash-landed) and clean proven; dirty paths, detached heads, unpublished
  branches, ignored files, and matching plan directories listed as judgment; shell-quoted
  removal commands printed and never run.
- Specs and inputs retire with their plans. The sweep checker classifies every file under
  `specs/` and `inputs/` as live, retiring, retirable, backlog (never consumed by any plan), or
  blocked, by citation of the file or its folder and by history; a `docs/` citation into either
  folder is a defect. With the register's parked rows, the survey is the backlog view; deletion
  stays engineer-confirmed in one sweep commit.
- `p-protocol-runbook` is a protocol utility that cuts an ideation handoff, register cluster, or
  audit reconciliation into a dated sequence of Tasks with dispatch prompts for both hosts, the
  landing policy chosen by the engineer, and objective-level completion named; a cut that is
  re-issued, never a ledger.
- The hook denies a new register id that already exists on `origin/main` as of the last fetch,
  including when a branch creates the register; the validator's terminal-lift check no longer
  skips a sub-bulleted lifted item; the validator test harness fails on a missing fixture
  instead of crediting a rejection.
- The README's release ritual names the Verify suites, an optional delta audit, the Codex
  rebuild, and both hosts' update commands. Two Codex audits ran on this line and their
  confirmed findings are remediated.

## 3.11.0 — 2026-10-21

- Deferred review adds opt-in `triage` and `verify <handoff-path>` modes in Claude Code and
  Codex. A bounded first pass saves per-row evidence and an explicit investigation queue under
  `inputs/`, leaving the register unchanged. Verification reconciles repository drift, checks
  transition candidates, spot-checks keeps, and retains engineer ratification. Model selection
  is manual between stages; the ordinary full review remains the default.

## 3.10.0 — 2026-10-20

- Codex Task support shares the core protocol, Markdown state, validators, and maintenance
  skills. `build-codex-package.cs` emits a relocatable local marketplace with only supported
  skills; Program and concurrent writers remain Claude-only.
- Hooks understand Codex multi-file patches, rename sources and targets, deletions, event cwd,
  and symlink paths. Shell snapshots use tool-call IDs when supplied so overlapping identical
  commands cannot consume each other's snapshots. Malformed hook input is rejected.
- The guarded launcher adds `--auditor claude` for cross-vendor audits of Codex work, with a
  Read/Glob/Grep-only tool surface and JSON report capture. Both backends stage output outside
  canonical audit state, retain failed output as rejected material, and refuse report overwrite.
- Host routing defines model handoffs, hook trust, recovery after post-tool rejection, and
  structured-question fallbacks. Explicit `no commits` instructions override checkpoints.
- Codex integration tests cover patch enforcement, persisted Task lifecycle, fake-Claude success
  and failure paths, manifest drift, report collisions, and execution from a relocated package.
  Live model adherence and installation trust remain installation smoke checks.

## 3.9.0 — 2026-10-19

Two engineer-reported frictions, both the same shape: the protocol said what to report and when
to ask, but gave neither a fixed form, so the agent composed position from memory and took the
softest exit on decisions. Rationale in `docs/design-rationale.md` decision 25.

- **The position line.** `validate-plan.cs --position <plan-dir-or-file>` validates, then prints
  one computed line per plan — `<plan> · <status> · steps 3/7 done · active STEP-04 · blocked
  STEP-06 · gates open GATE-02 · acceptance 2/5 · next STEP-05` (tracks count blocks, Programs
  count tracks; empty fields are omitted). Every boundary report, gate request, and completion
  response opens with it verbatim; under `continuous`, each checkpoint commit body carries it so
  `git log` reads as progress. Never composed by hand.
- **`needs-decision` asks before it parks.** A finding tagged `needs-decision` is a decision
  request first: the report that raises it closes with the decision interrogation — one batch,
  recommendation and default per item, through the host's structured question tool — and the
  finding becomes a register row only when the engineer defers the ruling. Under `continuous`
  the batch is held to the terminal report. The `deferred-review` skill re-presents surviving
  `needs-decision` rows the same way.
- Test baseline: plan-validator 80 → 85; hooks, sweep, launcher, registry unchanged.

## 3.8.0 — 2026-10-19

Two things landed together: deferral became a classification, and the protocol took its first
full external review of itself — two Codex cycles under `inputs/protocol-review/audits/`
(audit-01 scoped to ceremony and duplication, audit-02 undirected), each challenged on the pinned
top-model configuration. Rationale in `docs/design-rationale.md` decisions 23 and 24.

**Deferral is classified, never residual** (decision 23; engineer-reported: low-hanging findings
on surfaces already under edit were parked instead of fixed, because deferral needed no gate, no
step, and no verification rerun at any intake).

- **Finding triage in the Foundation.** One rule across every intake — confirmed audit finding,
  non-blocking failure, defect noticed on a surface under edit, open question. A finding that
  passes the **fix-now test** (surface already within scope or write surfaces; cause and repair
  understood; not material and extends no write surface; one reversible step with a check)
  becomes an ordinary step recording its origin, on the authority profile invocation already
  grants. A finding that fails is a deferred issue tagged with the failed condition:
  `defer: out-of-scope | needs-decision | cost | blocked-by`. Under `interactive` the next report
  lists the triage for the engineer to flip; under `continuous` fix-now proceeds.
- **One taxonomy from plan to register.** The tag is the register row's `Disposition`, with
  `accepted gap` as the fifth value; the old enum (`actionable | engineer's call |
  trigger-blocked | standing debt`) is retired from template, validator, tests, and the
  `deferred-review` skill.
- **Enforced on terminal plans.** A top-level item lifting to `→ D-NN` or `→ ISSUE-NN` must carry
  its tag or be an accepted gap, and the arrow must resolve to an existing register or ledger row.
  Closed items need no tag; live plans are unaffected until they complete.
- Assurance rule 7 remediates on the fix-now test; the audit challenge records a resulting action
  per confirmed finding (`remediate → STEP-NN`, `defer: <tag>`, `accepted gap → <gate>`); Task
  failure recovery and the Program ledger's classification line carry the tag.

**Protocol review remediations** (decision 24; provenance `inputs/protocol-review/audits/`).

- Program template no longer scaffolds `Program acceptance`, which the profile says to add only
  for cross-track criteria. [audit-01 F03]
- The readiness handoff is stated once, in the Foundation; the Task protocol and skill point at
  it. The challenge agent loads the policy's challenge contract by reference. [audit-01 F10, F11]
- Claims corrected to what enforcement can see: duplicate ids are rejected, renumbering across
  revisions is prose; the enforcement boundary (the host's Write, Edit, and Bash tools) is named
  in the Foundation and README. [audit-01 F07, audit-02 F01, F04]
- Validator: a `completed` plan may carry only satisfied or excepted acceptance, settled gates,
  and terminal work rows (Task, Program, track); track block rows reference mapped acceptance.
  [audit-02 F05, F08]
- Hooks deny `git remote update` and `git reset --soft`. Entry skills validate a resumed plan
  before any other work, catching a rejected write left behind by a dead session. [audit-02 F02,
  F03]
- Sweep checker: untracked content under a plan directory blocks "committed". [audit-02 F07]
- Launcher: a nonzero auditor exit refuses and parks partial output beside the launch log; the
  manifest is re-verified after capture and drift is stamped into the report. The launcher suite
  now exercises the live path with a fake auditor on PATH. [audit-02 F09, F10]
- Rebutted or already ruled: the register's Closed table (reserves ids, records reconciled
  closure), marker plus label (decision 6), validation on every write (decision 21), canvas
  stations (decision 14, optional), the assurance negative's form (a Decisions row).
- Two dialect simplifications, ruled by the engineer at the needs-decision interrogation:
  settled-gate strikethrough is accepted on a settled gate, never required; the `## Contents`
  index is optional and exact when present, so a stale index still fails. Templates drop the
  index; every existing plan stays valid. [audit-01 F04, F05; D-04, D-05 closed]
- Shell-side mutation is now checked: `pre-bash` snapshots `plan/` and `specs/`, and a new
  `post-bash` hook compares after the command — an audit report or bound spec changed through
  the shell is denied, a plan artifact changed or created through the shell is validated.
  Deletions are not flagged, because the retirement sweep deletes through that path. [audit-02
  F01; D-01 closed]
- Ids are stable against HEAD: a post-write (or post-bash) that drops a durable id present in
  the last committed version is denied once the committed plan is past `ready`; register D-ids
  from the first row. [audit-01 F07, audit-02 F04; D-02 closed]
- Program track rows resolve their acceptance ids into the referenced track plan's own table,
  and a referenced track plan must exist, for single-file validation too. [audit-02 F08; D-03
  closed]
- `plan/deferred.md` is created with the register's first rows; all five are closed by this
  release and remain as the reconciled record.
- Test baseline: plan-validator 61 → 80, hooks 38 → 53, sweep 8 → 9, launcher 8 → 16, registry
  unchanged.

## 3.7.0 — 2026-08-22

The readiness model handoff recommends its execution model. Engineer-reported friction: the
downshift boundary (3.6.0) handed over the resumption invocation but left "which model?" to be
asked at nearly every handoff. Rationale recorded as `docs/design-rationale.md` decision 22.

- The Foundation's model handoff now opens with the **execution-model recommendation** — the
  routing rule ("lowest-cost configuration demonstrated capable") applied to the plan just
  written: the concrete recommended model, the demand signals driving it (remaining exploration
  or design judgment, blast radius and reversibility of the riskiest steps, how reliably the
  selected verification would catch a weaker model's errors), confidence, and flip conditions —
  the decision-ready shape of 3.4.0's profile recommendation. A step already known to exceed the
  recommended configuration is named with its capability escalation rather than raising the whole
  session's tier.
- Advisory only: the engineer selects the execution model by launching the fresh session, and the
  concrete model name is report output — agent-type definitions remain the only Protocol source
  pinning model identity.
- Task readiness prose and the Task entry skill restate the contract; Program and track readiness,
  material replanning, and spec supersession inherit it through the Foundation's handoff
  definition unchanged. README updated (readiness paragraph and flowchart).
- Prose-level rule: no artifact-dialect, template, or enforcement change. Test baseline
  unchanged.

## 3.6.0 — 2026-08-17

Retro-driven remediations from the first heavyweight deployed Task (provenance:
`inputs/protocol-cost.md` plus engineer observations — ignored cadence, unclear doneness, model
choice reopened, clerical deferred moves). Rationale recorded as `docs/design-rationale.md`
decisions 18–21.

- **Planning on the top model; readiness is the downshift boundary.** Profile entry through
  execution readiness belongs on the top-model configuration, carried by the session model the
  engineer selects at entry. Execution readiness is now a declared boundary in every cadence —
  STEP-01 never starts on planning's momentum: the report declares planning complete and hands
  the exact resumption invocation for a fresh session on the intended execution model;
  proceeding in place is the ordinary release. Material replanning and spec supersession surface
  the same handoff. The pinned challenge agent type the Task protocol referenced now actually
  ships (`protodog/agents/challenge.md`, `model: fable`) — the one place model identity is
  written down; previously the top-model challenge pin could not execute.
- **Spec premises verified before binding.** Material claims about current repository state are
  checked against the repository before a spec binds; unverifiable claims become assumptions.
  Immutability locks only what grounding has checked.
- **Completion closes declaratively.** The final response opens by declaring the profile
  completed and closes with the remaining engineer-owned actions: the commit to main, the
  retirement sweep, and the offered deferred-register review against the finished work — offered,
  never run unprompted (a degraded closing context is the engineer-ruled reason; a fresh session
  serves the review better). The lift rule now states lifting is judgment, not reflex.
- **Assurance timing.** New trigger: changed runtime composition, configuration, environment, or
  external wiring schedules the thinnest executable end-to-end run early — configuration-class
  defects are observable only by running. And the audit freeze window: manifested artifacts are
  frozen from launch to report capture, queued plan transitions flush after; the Foundation names
  this as the plan-currency rule's one exception (previously the two rules conflicted silently).
- **Vocabulary: "landing" → the commit to main** (engineer-ruled — explicit about the intent;
  bare "commit" stays with checkpoint commits). Propagated across core, skills, hooks messages,
  sweep-check output ("commit-then-sweep", "terminal, lifted, committed"), and README. Historical
  changelog entries keep the old term.
- **Validator diagnostics** (write-time validation deliberately kept; the report's
  step-boundary recommendation declined): the terminal-lift arrow rule now reads wrapped items
  whole — the disposition arrow may close the lead line or the item's final line — and its
  message says where the arrow sits; status and cadence enum rejections list the valid values.
- Test baseline: plan-validator 61 → 63 (wrapped-item cases); sweep-check 8 with re-termed
  expectations; registry 11, hooks 38, launcher 8 unchanged.

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
