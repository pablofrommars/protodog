# audit-02-challenge — challenge of audit-02 against the pinned state

- Challenge owner: coordinating session (protodog-31), Fable 5.1 — the pinned top-model configuration.
- Target state: commit `53e9101` plus the uncommitted 3.8.0 working tree, identity by the dispatch manifest (unchanged from launch to capture; the manifested files were not written during the window).
- Owning plan: none. This review runs outside a profile. Resulting actions land in the engineer-reviewed release commit, in the deferred register, or at a decision gate, as named per finding.
- Authorship check: this session authored the 3.8.0 changes under audit; the audit was independent, and this challenge is the author's rebuttal.

The report found no ceremony; every finding is a gap between a claimed mechanical guarantee and what the enforcement code can see. Dispositions apply the Foundation's finding triage: fix-now where one bounded, checked, reversible step closes it; `defer: <tag>` otherwise.

## Dispositions

### FINDING-01 — Write fences bypassable through Bash
**Disposition: confirmed.** Routing is as cited: Bash reaches only the Git deny list. A shell redirect or in-place edit mutates a bound spec, an audit report, or a plan without the validator running. The Foundation and README say "(enforced)" without naming the tool boundary. Material: this is the protocol's central enforcement claim. Resulting actions: **remediate → the prose discloses the boundary** (enforcement covers the host's Write and Edit tools; shell-side mutation of canonical artifacts is prohibited, not detected). Mechanical closure — a post-Bash pass that validates changed plan files and diffs immutable artifacts against HEAD — fails the fix-now test on cost and is recorded **`defer: cost`**.

### FINDING-02 — Git deny list misses `remote update` and `reset --soft`
**Disposition: confirmed.** Both forms perform engineer-owned operations and neither matches a deny pattern. Resulting action: **remediate → add the two patterns and their hook tests.** The deny list stays lexical by design (decision 5); semantic completeness is not claimed.

### FINDING-03 — Post-write validation is not transactional
**Disposition: confirmed, consequence overstated.** No rollback exists; the denial reaches the model, which repairs the artifact in the ordinary case. The residual case is a session dying between rejection and repair, leaving invalid state for the next resume. Resulting action: **remediate → entry skills validate the plan on resume** (one step in the Task and Program skills), which catches the residue at the only moment it matters. Rollback machinery is not added.

### FINDING-04 — Stable-ID non-renumbering not validated
**Disposition: confirmed; duplicate of audit-01 FINDING-07.** Same resulting actions: prose corrected now, hook-side check against the last committed version recorded `defer: cost` in the audit-01 challenge.

### FINDING-05 — Contradictory terminal state validates and becomes sweepable
**Disposition: confirmed.** The validator checks the status enum and row grammar, never that a `completed` plan's acceptance is `satisfied` or `accepted exception`, its gates settled, and its work rows terminal. Resulting action: **remediate → validator check on `completed` plans** for Task, Program, and track plans, with tests. Sweep-check inherits it: a plan that cannot validate cannot have been written through the hook.

### FINDING-06 — Arrow destination never resolved
**Disposition: confirmed.** `→ D-99` with no such register row passes. Resulting action: **remediate → the validator resolves `→ D-NN` against `plan/deferred.md` and `→ ISSUE-NN` against the owning program plan's ledger** when the destination file exists, with tests. This forces register-first write order on lift, which is the intended order.

### FINDING-07 — "Committed" proof excludes untracked content
**Disposition: confirmed.** `git diff --quiet` cannot see untracked files. Resulting action: **remediate → sweep-check also requires an empty untracked listing under the plan directory**, with a test.

### FINDING-08 — Program acceptance ownership not referentially validated
**Disposition: split.** Track-plan block rows: **confirmed** — the Acceptance cell is parsed and ignored while step rows in the same file are checked; **remediate → check block rows against the track's acceptance table.** Program track rows: **overstated** — acceptance ids are defined by bound specs, not by the optional Program acceptance table (cross-track reconciliation only), so the comparison the report implies would reject correct plans. A cross-file check against each track plan's acceptance table once that plan exists is possible; it fails the fix-now test on cost and is recorded **`defer: cost`**.

### FINDING-09 — Freeze window not preserved by the launcher
**Disposition: overstated as to claim, confirmed as to gap.** The assurance policy states the freeze as a prose rule and carries no "(enforced)" tag; the Foundation names it as a transition rule no validator can see. The report is right that drift is undetected. Resulting action: **remediate → the launcher re-verifies the manifest after capture; on mismatch it stamps the report with the drifted paths and logs DRIFT**, so the challenge treats the report as bounded by that limitation. Refusing would discard a paid report; stamping keeps it inspectable. A lock in the hooks is not added.

### FINDING-10 — Failed auditor captured as a successful report
**Disposition: confirmed.** The exit code is logged, never gated. Resulting action: **remediate → nonzero exit refuses; the partial output moves to the launch-log directory as rejected material, never into `audits/`.** The launcher test gains a fake auditor on PATH, which also makes the success path testable for the first time.

## Bounded search for misses (all dimensions)

No material miss found beyond the report. Two observations recorded as not-findings: the `pre-bash` deny patterns stop at `|`, `;`, and `&`, which is correct for chained commands and makes no claim against obfuscated spellings, and the register validator is reached only at the exact path `plan/deferred.md`, matching the Foundation's one-register rule.

## Deferred

- Post-Bash validation pass for canonical artifacts (FINDING-01) · `defer: cost` — exceeds one bounded step; revisit when a shell-side mutation of a canonical artifact is observed, or when the hooks are next reworked.
- Cross-file check of Program track rows' acceptance ids against track plans (FINDING-08) · `defer: cost` — revisit with the FINDING-06 destination resolution if Program use shows unowned criteria.

## Gates returned to the engineer

None from this report. The audit-01 challenge's two dialect decisions stand.
