# Verification and Assurance Policy

## Why this document exists

This defines how Protodog, the Claude-native agentic execution protocol selects verification and additional
assurance for Task and Program execution, and owns the audit cycle end to end. It supersedes
`bundle/verification-and-assurance.md`; the invariants are unchanged, the audit loop is updated for
planning-time selection and the guarded external launcher. It gives agents a consistent internal
policy without turning assurance into an engineer-operated lifecycle or a numeric risk score.

Status: committed normative policy, Claude-native line.

## Role in execution

The selected profile applies this policy during grounding, planning, execution, replanning, and
completion; there is no separate assurance plan or decision-tree artifact. Record an assurance
choice in the owning plan only when it affects acceptance or required verification; sequencing,
dependencies, or integration; cost, external effects, or authority; a controlled measurement or
evaluation run; or a selected review or audit. Ordinary internal selection creates no HIL; use the
Foundation interface only when the selection requires engineer authority or judgment.

## Universal verification invariants

1. **Acceptance is mapped to evidence.** Every bound criterion identifies how it will be
   established. One check may support several criteria and vice versa; no duplicate checks for
   formatting symmetry.
2. **Verification is proportionate and discriminating.** Select the smallest set of checks that can
   credibly detect a relevant failure; broaden only for affected contracts, propagation paths,
   integration seams, or risks narrower checks cannot cover.
3. **Evidence applies to an identified state.** Record the check performed, its material result,
   and repository, system, configuration, or run identity when needed to know what the result
   supports.
4. **A pass is never manufactured.** Do not weaken tests, evaluations, validation, cases, rubrics,
   thresholds, or acceptance criteria to obtain a pass. A justified requirement change follows the
   owning spec and gate rules.
5. **Claims do not outrun execution.** Name an unexecuted check as planned or omitted, never
   passed. Do not infer full-suite success from a selection, a build from one target, or runtime
   behavior from static inspection alone.
6. **New behavioral checks earn trust fail-first.** A new or changed check claiming new or changed
   behavior is observed failing for its intended condition, then passing. Not applicable to
   unchanged checks, builds, lints, observational measurements, or test-only restructuring; when
   fail-first would be unsafe or uninformative, disclose the omission and rationale.
7. **Failures are classified before remediation.** `new / pre-existing / environmental /
   unresolved`, with intermittence recorded separately. Change the implementation only when cause
   and repair are understood and in scope; rerun affected verification.
8. **Retries require information.** Another attempt requires remediation, a new hypothesis, new
   evidence, or a relevant state change.
9. **Evidence remains concise and inspectable.** Identify results and link retained output; keep
   detail only for failure analysis, comparability, evaluation identity, audit, or resumption.
10. **Completion discloses its limits.** Materially omitted verification and its consequence are
    disclosed; an omission leaving acceptance unmet requires an engineer-approved accepted
    exception — an agent cannot downgrade the requirement or accept the exception itself.

## Assurance selection

Apply internally, never as a questionnaire: bind the criteria, invariants, and claims the work must
support; choose the cheapest credible checks that can falsify each material claim; apply the
triggers below; order cheap discriminating feedback before expensive execution when that does not
compromise a baseline or measurement window; persist only execution-relevant choices; re-evaluate
affected assurance after a material delta, failed check, audit finding, remediation, integration,
or target-state change.

| Trigger | Required response |
|---|---|
| A new or changed check claims new or changed behavior | Fail-first, then pass against the implementation; record both with the affected step. |
| A change can affect an existing invariant, public contract, compatibility surface, data shape, security boundary, or downstream consumer | Run the relevant existing cases and broaden across the affected propagation path; a selection remains a run, not a suite. |
| Static checks cannot establish a claimed runtime, performance, reliability, or operational effect | Use an executable check or controlled measurement; pin baseline, window, and comparability conditions when comparison matters. |
| Work introduces or changes runtime composition, configuration, environment, or external wiring | Schedule the thinnest executable end-to-end run early in execution — before feature work builds on the composition, not after structural green. Configuration-class defects are observable only by running, and one deliberately trivial run reaches them while they are cheap. |
| The outcome depends on model behavior or a governed quality rubric | Use an evaluation run pinned to system/model/configuration state; paid or external execution follows its authorization gate. |
| Independently produced work reconciles, or a result crosses a concurrency or integration seam | Verify each bounded result before reconciliation and the combined state after, for interactions isolation cannot detect. |
| A planning-time selection, spec, engineer instruction, or repository policy requires a review or audit | Execute against exact scope and pinned state through the contracts below. |
| A material verification item cannot execute or cannot support its claim | Mark it omitted or unresolved with reason, consequence, and what would settle it; open an accepted-exception request if acceptance is left unmet. |

**The assurance interrogation.** Scheduling of the authorization-gated assurance instruments —
audits and paid or external evaluation runs — is settled at the start of planning, not discovered
downstream. Task planning, and each track's planning in a Program, opens by interrogating both
for its scope: is one selected, by which authority, over what scope, with what placement or run
bounds? Exactly one outcome per instrument is persisted in the owning plan before execution
readiness: a selected audit with its declared placement, or scheduled evaluation runs with
purpose, order, and bounds; a decision gate raised at planning entry when the indicators support
an instrument that no selecting authority has ruled on — never silently selected, never silently
dropped; or the explicit not-scheduled outcome, recorded as one decision with its basis. The
negative is recorded deliberately: an absent answer is indistinguishable from an unasked
question. Program-wide scheduling is interrogated once at Program planning; a track's
interrogation covers its assigned scope and may conclude that a Program-wide placement already
carries it. The interrogation settles scheduling only — each launch keeps its own authorization
gate — and later evidence reopens the outcome as an ordinary material delta: it removes the
uncertainty, not the ability to change.

## Test, measurement, and evaluation identity

A **test case** is one defined automated check; a **test suite** is the complete collection within
a named scope; a **test run** executes the suite or an explicitly identified selection — never
called a suite. A **baseline** identifies the code, configuration, inputs, environment, and prior
result used for comparison; a **measurement window** controls the baseline-to-after interval; a
comparison is valid only as far as its material conditions are comparable or disclosed. An
**evaluation case / suite / run** follows the same structure against pinned system, model, and
configuration state; evaluation-run identity includes run ID, suite version, scope, configuration
identity, repository-state provenance, environment, start time, and result location.

**Measurement policy.** An absolute threshold needs a pinned target state and method; improvement,
regression, or causal claims need a controlled or disclosed-comparable baseline and window. A
failure is `new` only when evidence ties it to current work and shows absence at a suitable
baseline — otherwise `unresolved`. Define the metric, rubric, threshold, or comparison before an
acceptance measurement; exploratory measurement cannot retroactively become its own pass threshold.
Repeat runs only when variability matters; report the distribution; never retry until a favorable
sample appears.

**Evaluation policy.** Pin the suite or exact case selection and full configuration before
execution; material changes require a distinct run identity. Bind thresholds before an acceptance
run; diagnostic runs may stay exploratory. Designed model variability is observed behavior, not
automatically a failure. Paid or external evaluation is authorization-gated: the request identifies
the exact selection, configuration, purpose, run and retry bounds, cost bound, transmitted data
class, result destination, and what waits. One authorization may cover a bounded series only when
those bounds are explicit. Run before remediation for diagnosis or baseline, after for acceptance,
both for controlled comparison; the owning plan records purpose, order, authorization, and run
identity. Scheduling of paid or external runs is settled at planning entry by the assurance
interrogation; each launch keeps its authorization gate.

## Profile assurance

**Task.** The universal invariants and triggers, no second layer. Map every bound criterion to
planned verification before `ready`; verify each step at the narrowest credible level; establish
acceptance against the final Task state, reusing evidence only when later changes left its claim
and target unaffected; verify reconciled concurrent results for interactions not covered in
isolation. Task selection alone never requires a full suite run, evaluation run, review, audit, or
independent executor.

**Program.** Adds verification across responsibility and integration seams: each track applies the
Task policy to its assigned result; track-local evidence establishes only claims within the
verified track state; dependency seams are verified before a dependent track consumes a
predecessor; combined states are verified after reconciliation; a track closes only with evidence
or accepted exception in the state the Program will consume; completion establishes acceptance
against the combined integrated state. Progressive planning may defer detailed checks with
not-yet-planned work but cannot hide acceptance ownership. Multiple tracks alone require no added
assurance.

## Review and audit selection

A **review** is evidence-based inspection, performable by the current agent as ordinary in-scope
assurance; it creates no artifact or HIL unless another actor needs the result.

An **audit** adds independence and an adversarial posture, and exists only when selected **at
planning time** — by the engineer, spec context, repository policy, or governing assurance policy —
and recorded in the owning plan with its declared placement (a lifecycle position or state, e.g.
"post-integration of TRACK-02, blocks the commit to main"). An emergent audit need is a material
delta and
enters through a plan update. Blast radius, uncertainty, irreversibility, or weak direct
verification may support a recommendation; they never silently select an audit. Selection is
settled at planning entry by the assurance interrogation above.

Every audit is independent of the audited authorship; adversarial toward claims but symmetric about
the result; scoped to exact artifacts and pinned target state; undirected within that scope;
grounded in inspectable evidence; and read-only except for its report. Independence requires a
separate session with no authorship of the audited state, the challenge, or remediation.

**The standing auditor is the external engine** (cross-vendor: currently Codex with its pinned top
model), selected for model-family diversity at the trust boundary; the launcher pins and the report
discloses the exact auditor configuration. Unavailable diversity is disclosed as an audit
limitation.

## Audit cycle

The numbered cycle within a declared scope is `audit-NN-dispatch.md`, `audit-NN.md`,
`audit-NN-challenge.md`; later audits and re-audits increment, never overwrite.

**Dispatch.** When execution reaches the declared placement, persist the dispatch: self-contained
and executable without prior conversation, installed Protocol instructions, or registry prompts. It
contains audit identity and output destination; independence requirement; declared scope with exact
artifact and repository-state identity (prefer a checkpoint SHA; otherwise content hashes);
normative-source manifest and separately labeled neutral references; evidence locations and
inspection constraints; the standard dimensions and report contract; authority boundaries and
prohibitions; and the coverage-disclosure requirement.

**Launch** is an authorization gate: the decision-ready request identifies the dispatch path,
pinned state, auditor configuration, cost bound, and transmitted data class (repository content
leaves the boundary). On approval, the guarded launcher — taking exactly the dispatch path as its
only argument — re-verifies the manifest hashes and refuses on mismatch, invokes the external
engine read-only in the background with the pinned configuration, and captures the report to the
next unused `audit-NN.md`, immutable once written (enforced). From launch to report capture, the
artifacts pinned in the dispatch manifest are frozen: no write touches a manifested file while
the auditor reads, and a plan transition that would is queued and flushed immediately after
capture. This is the named exception to the Foundation's plan-currency rule — a current plan and
a pinned audit target are otherwise in direct conflict, and the pin wins for exactly that window.
A re-launch is a new numbered cycle; the challenge must address every report on file. The
engineer-driven manual launch path remains as fallback with the same dispatch contract.

**Standard dimensions** (considered where applicable, never a scorecard): outcome and acceptance
correctness and completeness; internal and cross-artifact consistency including state and
vocabulary integrity; evidence, verification, measurement, and evaluation integrity; compliance
with scope, constraints, invariants, authority, decisions, and applicable policy; failure,
recovery, concurrency, integration, security, data, operability, and maintainability risks;
unnecessary complexity, ceremony, or duplication.

**Report.** `audit-NN.md` states scope and exact target-state identity; sources actually used;
inspected coverage, inaccessible surfaces, limitations, and material methods; a concise overall
assessment bounded by that coverage; and zero or more atomic findings — each a stable `FINDING-NN`
with claim, evidence, affected surface, consequence, and `high / medium / low` confidence tied to
evidence. No mandatory severity scale; recommendations optional and advisory. A clean audit covers
only the inspected scope.

**Challenge.** The challenge owner — the Task coordinating agent for a Task cycle, the planner for
a Program cycle — challenges every finding against the pinned state on the top-model configuration
and writes `audit-NN-challenge.md`: per finding, a disposition
(`confirmed / overstated / false positive / deliberate design / unverifiable here`), exact
challenge evidence and corrected claim where applicable, material consequence and required gate if
any, and resulting owning-plan steps, verification, or no-action rationale. The challenge also
performs a bounded search for material misses within the declared scope, recorded under the next
unused `FINDING-NN`. The auditor never performs its own challenge. Confirmed work becomes ordinary
plan steps; all remediation uses the top-model configuration; a disposition changing outcome,
scope, acceptance, or authority, or creating an accepted gap, returns to the engineer through its
gate; affected verification reruns after remediation.

**Re-audit** is required only when the audit is a bound acceptance requirement invalidated by
confirmed material findings; a material inspection limitation is later removed; remediation or
another delta invalidates the assurance the audit was meant to provide; or an explicit selection
requires it. Targeted confirmation of known remediation is verification or directed review. A
re-audit receives the newly pinned state and the same neutral contract without prior cycle
artifacts unless they are explicitly in scope. There is no universal fresh-executor rule for
remediation; use a fresh session only when capability, isolation, responsibility separation, or
context health makes it useful.
