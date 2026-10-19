# audit-01-challenge — challenge of audit-01 against the pinned state

- Challenge owner: coordinating session (protodog-31), Fable 5.1 — the pinned top-model configuration.
- Target state: commit `53e9101` plus the uncommitted 3.8.0 working tree, identity by the dispatch manifest (unchanged since launch; verified by the launcher).
- Owning plan: none. This review runs outside a profile. Resulting actions land in the engineer-reviewed release commit or in a later Task, as named per finding.
- Authorship check: this session authored the 3.8.0 changes under audit. The audit itself was independent (external engine, pinned, read-only); this challenge is the author's rebuttal and is labeled as such per the assurance policy.

## Dispositions

### FINDING-01 — Closed table in the deferred register
**Disposition: overstated.**
Evidence holds as cited: the template mandates `## Closed`, the validator requires it, the Foundation says `plan/` is never an archive. Corrected claim: the Closed table has two live consumers the report did not weigh. It reserves D-ids, which are never reused or renumbered (`deferred-review/SKILL.md` step 3); without it the next id must be derived from Git history. And it is the reconciled-closure record the review exists to hunt (the register's founding defect was silent closure, decision 12). Cost is one line per closed row. Consequence not material. Resulting action: none.

### FINDING-02 — Two deferral taxonomies
**Disposition: confirmed.**
The 3.8.0 `defer:` tags (`out-of-scope | needs-decision | cost | blocked-by`) and the register's `Disposition` enum (`actionable | engineer's call | trigger-blocked | standing debt`) classify the same obligation twice with no mapping, and the validator accepts contradictory pairs. Introduced by this session's own 3.8.0 change. Material consequence: duplicate classification on every lift. Resulting action: **remediate → one taxonomy.** The register's `Disposition` cell takes the lift's tag (`out-of-scope | needs-decision | cost | blocked-by | accepted gap`); the old enum is retired from the template, the register validator, its tests, and the `deferred-review` skill. Lands in the 3.8.0 release commit, after the audit-02 freeze lifts.

### FINDING-03 — Program template scaffolds Program acceptance
**Disposition: confirmed.**
`templates/program-plan.md` carries a `Program acceptance` section and contents entry that `program-protocol.md` says to add only for cross-track criteria, under the Foundation's enforced omit-absent-sections rule. Resulting action: **remediate → drop the section and its contents entry from the template.** Verify the validator accepts the two-section kernel and update the template fixture test if it pins the old shape. Lands in the 3.8.0 release commit.

### FINDING-04 — Marker, label, and strikethrough
**Disposition: split.** Marker plus label: **deliberate design** — decision 6 records the reason (scannable glyph plus authoritative enum, drift made impossible by validation), and the observed cost is one enum rejection. Settled-gate strikethrough: **confirmed** — a third encoding of the same closure with no rationale on record beyond "reserved for settled gates". Resulting action: **remediate, gated.** Proposal: make strikethrough accepted but not required on settled gates (validator), drop it from the Foundation grammar and README. Dialect change, so it returns to the engineer as a decision before it lands.

### FINDING-05 — Mandatory Contents index
**Disposition: deliberate design, with the finding's cost accepted as real.**
Decision 6 records why the index is validated: an enforced invariant instead of decoration that rots. The report is right that a two-section plan pays authoring and validation for no navigation value. Resulting action: **engineer decision.** Proposal: `## Contents` becomes optional; when present it must be exact (validator unchanged otherwise). Returned to the engineer as a decision.

### FINDING-06 — Validation on every write
**Disposition: deliberate design.**
Decision 21 weighed exactly this evidence (`inputs/protocol-cost.md`, three clerical rejections) and declined to move cadence to step boundaries. The report adds no new evidence. Resulting action: none; the engineer may reopen decision 21.

### FINDING-07 — "Renumbered IDs are rejected mechanically" cannot fire
**Disposition: confirmed.**
The validator sees only the current artifact (`validate-plan.cs` step-row and directory walks); it rejects duplicates and malformed ids, never a renumbering against a prior state. `task-protocol.md` and decision 6 claim otherwise. Resulting action: **remediate → correct the prose** in the Task protocol and rationale to what is enforced (format, uniqueness, mapped references); lands in the 3.8.0 release commit. A hook-side check against the last committed version of the file would make the claim true; it fails the fix-now test on cost and is recorded as **`defer: cost`** below.

### FINDING-08 — Assurance interrogation's negative outcome has no canonical form
**Disposition: overstated.**
The policy names the form: "recorded as one decision with its basis", so it is a `## Decisions` row, an existing optional section. Unenforced is correct; the rationale (decision 15) accepts prose rules for judgment. The repeated explicit negative is the engineer's stated choice. Resulting action: none.

### FINDING-09 — Working canvas across stations
**Disposition: overstated.**
The canvas is optional: the Foundation says a diagram-suited session "may keep" one, and the skill is engineer-invoked. Nobody is forced to maintain a second representation. The report's narrower point holds: the planning and execution stations were adopted as "foreseen" (decision 14), not from evidence. The registry duplication is covered by the self-containment rule the report itself accepts under clean areas. Resulting action: none; the engineer may revisit decision 14 after a Program-scale session produces evidence either way.

### FINDING-10 — Readiness handoff restated in three layers
**Disposition: confirmed.**
`skills/task/SKILL.md` declares it carries only invocation mechanics and then restates the Foundation's handoff contract; `task-protocol.md` restates it again; 3.7.0's changelog records the triplication. Resulting action: **remediate → collapse to one statement.** The Task skill's step 5 and the Task protocol point at the Foundation's handoff definition instead of restating its fields. Lands in the 3.8.0 release commit.

## Bounded search for misses (ceremony dimension)

### FINDING-11 — The pinned challenge agent restates the assurance policy's challenge contract
Claim: `agents/challenge.md` is described as "the one place the protocol pins current top-model identity", yet it carries the full challenge contract (disposition enum, evidence fields, gate rule, bounded miss search) duplicated from `verification-and-assurance.md`, and 3.8.0 had to edit both for one grammar change. Evidence: the two passages are near-verbatim. Affected surface: assurance policy, agent definition, every future grammar change. Consequence: the same drift risk FINDING-10 names. Confidence: high. Resulting action: **remediate → the agent loads the policy by reference**, as the entry skills do, and keeps only identity, model, and invocation shape. Lands in the 3.8.0 release commit.

No further material miss found within the dimension on the surfaces the report left clean.

## Deferred

- Hook-side renumbering check against the last committed version of a plan file (would make the FINDING-07 claim true) · `defer: cost` — exceeds one bounded step; revisit when a renumbering is observed in practice or when the hooks are next reworked.

## Gates returned to the engineer

- FINDING-04: make settled-gate strikethrough optional (dialect change).
- FINDING-05: make `## Contents` optional, exact when present (dialect change).

## Verification after remediation

All five suites rerun; the audit-02 report, still in flight at the time of this challenge, is challenged separately and may add to this list.
