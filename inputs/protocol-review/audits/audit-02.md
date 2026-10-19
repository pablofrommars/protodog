<!-- launched by audit-launch.cs: dispatch audit-02-dispatch.md sha256=7b5b517b3fd5db909bdc64b66282b3f8e90c904878a6e25fc6de9e009790b238 model=gpt-5.6-sol sandbox=read-only -->
# audit-02 — Independent adversarial audit of the Protodog protocol

## 1. Scope and target state

This audit covered every standard dimension declared in `audit-02-dispatch.md`, without directed emphasis, across the complete manifested Protodog protocol set: normative core, enforcement, entry points and mechanics, prompt registries, operations guide, and design rationale.

The audited target state was commit `53e9101` on `main` plus the uncommitted release 3.8.0 working-tree changes identified by the dispatch manifest. All 39 manifested source and neutral-reference SHA-256 hashes were independently recalculated and matched the dispatch.

The engineer’s other repositories, model-vendor behavior, and files outside the manifest were out of scope. The absence of `plan/` was treated as expected and not as a finding.

## 2. Sources used

The dispatch itself and every manifested file were read.

**Normative core**

- `protodog/core/foundation.md`
- `protodog/core/git-policy.md`
- `protodog/core/program-protocol.md`
- `protodog/core/task-protocol.md`
- `protodog/core/verification-and-assurance.md`

**Enforcement**

- `protodog/hooks/hooks.json`
- `protodog/hooks/protodog-hooks.cs`
- `protodog/hooks/settings-fragment.json`
- `protodog/hooks/test-hooks.sh`
- `protodog/scripts/audit-launch.cs`
- `protodog/scripts/sweep-check.cs`
- `protodog/scripts/test-audit-launch.sh`
- `protodog/scripts/test-sweep-check.sh`
- `protodog/validators/test-registry.sh`
- `protodog/validators/test-validators.sh`
- `protodog/validators/validate-plan.cs`
- `protodog/validators/validate-registry.cs`

**Entry points and mechanics**

- `protodog/.claude-plugin/plugin.json`
- `protodog/agents/challenge.md`
- `protodog/skills/deferred-review/SKILL.md`
- `protodog/skills/ideation-audit/SKILL.md`
- `protodog/skills/plan-sweep/SKILL.md`
- `protodog/skills/program/SKILL.md`
- `protodog/skills/task/SKILL.md`
- `protodog/skills/working-canvas/SKILL.md`
- `protodog/templates/deferred-register.md`
- `protodog/templates/dispatch.md`
- `protodog/templates/program-plan.md`
- `protodog/templates/spec.md`
- `protodog/templates/task-plan.md`
- `protodog/templates/track-plan.md`

**Prompt registries**

- `protodog/registries/boxer.code-snippets`
- `protodog/registries/prompt-registry-style.md`
- `protodog/registries/prompt-utility.code-snippets`

**Operations and rationale**

- `README.md`
- `docs/design-rationale.md`

**Neutral references**

- `CHANGELOG.md`
- `inputs/protocol-cost.md`
- `inputs/ideation-visual-support-handoff.md`

## 3. Coverage

The audit inspected:

- Outcome, acceptance, lifecycle, authority, cadence, state, and vocabulary semantics across the five core documents.
- Cross-artifact consistency among core documents, skills, templates, README, rationale, and registries.
- Hook routing and command matching.
- Plan, spec, dispatch, deferred-register, and registry validation logic.
- Audit-launch target pinning, process handling, and report capture.
- Retirement-sweep terminal, lift, and Git-state checks.
- Behavioral-test coverage relative to the guarantees claimed by the implementation.
- Failure, recovery, concurrency, integration, security, data-egress, operability, maintainability, and ceremony risks where applicable.

Material methods were full static reading, SHA-256 manifest verification, control-flow tracing, comparison of normative claims with enforcement paths, and adversarial construction of states or commands accepted by the inspected logic.

No manifested file was inaccessible. The five test suites were not executed because the dispatch forbids creating or modifying files, while the drivers create temporary fixtures and some append repository or audit logs. Consequently, host-level hook behavior and executable test results were not dynamically observed. Network access and external model behavior were not inspected.

## 4. Overall assessment

Within the inspected static coverage, Protodog’s semantic core is unusually consistent about authority, provenance, cadence, profile separation, acceptance, and evidence. The principal weakness is the gap between those semantics and the stronger claim that mechanically decidable invariants are enforced. Ordinary tool paths can bypass canonical-state fences; several lifecycle and cross-artifact invariants are not checked; retirement can certify incomplete or unarchived state; and the audit launcher does not fully preserve the target and report integrity its contract asserts.

These gaps do not invalidate the prose methodology, but they mean canonical state, retirement safety, Git authority, and audit assurance cannot presently be treated as mechanically guaranteed.

## 5. Findings

### FINDING-01 — Canonical-state write fences are bypassable through the Bash tool

**Claim:** Bound-spec immutability, audit-report immutability, and plan/register validation apply only to `Write|Edit`; ordinary file mutation through `Bash` bypasses all three fences.

**Exact evidence:**

- `protodog/hooks/hooks.json:3-17` routes `Bash` only to `pre-bash`, while `pre-write` and `post-plan-write` match only `Write|Edit`.
- `protodog/hooks/protodog-hooks.cs:96-120` shows that `PreBash` checks only a list of Git command patterns and performs no file-write, spec, audit, or plan validation.
- Bound-spec and audit-report checks exist only in `PreWrite` at `protodog/hooks/protodog-hooks.cs:122-159`.
- Plan/register validation exists only in `PostPlanWrite` at `protodog/hooks/protodog-hooks.cs:162-186`.
- The affected guarantees are described as enforced in `protodog/core/foundation.md:84-89`, `protodog/core/foundation.md:156-157`, and `README.md:26-29`.

**Affected surface:** Bound specifications, audit reports, task/program/track plans, dispatches, and `plan/deferred.md`.

**Concrete consequence:** A shell editor, redirection, formatter, script, or other Bash command can modify an immutable artifact or leave malformed canonical state without invoking the corresponding check.

**Confidence:** High — the routing and Bash handler are explicit, exhaustive control-flow evidence.

### FINDING-02 — The Git hook does not enforce the full engineer-owned operation class

**Claim:** The lexical deny list permits ordinary Git operations that perform engineer-owned synchronization or history rewriting.

**Exact evidence:**

- `protodog/core/git-policy.md:26-31` reserves remote synchronization and all history rewrites to the engineer.
- `protodog/hooks/protodog-hooks.cs:98-111` denies named spellings including `fetch`, `pull`, `rebase`, `filter-branch`, `commit --amend`, and only `reset --hard`.
- The list does not cover `git remote update`, which fetches remote refs, or `git reset --soft`, which moves the current branch tip and rewrites its history.
- `protodog/hooks/test-hooks.sh:22-35` tests only the listed spellings and contains no semantic-equivalence cases.

**Affected surface:** Git authority boundary and recoverability of managed profile branches.

**Concrete consequence:** An agent can synchronize remote state or rewrite branch history through commands the hook allows, despite those actions being explicitly engineer-owned.

**Confidence:** High — the policy category and omitted valid Git forms are directly observable.

### FINDING-03 — Failed post-write validation is not transactional

**Claim:** A validator rejection occurs after the artifact has been written and the hook contains no mechanism to restore the previous valid contents.

**Exact evidence:**

- `protodog/hooks/hooks.json:13-17` installs validation under `PostToolUse`.
- `protodog/hooks/protodog-hooks.cs:162-186` launches the validator after the write and calls `Deny` on failure; `Deny` only logs, prints an error, and exits at `protodog/hooks/protodog-hooks.cs:72-94`.
- No prior contents are captured and no rollback or quarantine operation exists.
- `protodog/hooks/test-hooks.sh:51-56` asserts only that the hook returns denial for an already-mutated file; it does not assert restoration of valid contents.

**Affected surface:** Every canonical plan, dispatch, or deferred-register write performed through `Write|Edit`.

**Concrete consequence:** A failed edit can leave invalid canonical state on disk if the session terminates, crashes, or does not successfully repair the artifact after the rejection.

**Confidence:** High for the absence of rollback in the implementation; host-level recovery behavior was not dynamically tested.

### FINDING-04 — Stable-ID non-renumbering is claimed as mechanical but is not validated

**Claim:** The validator cannot reject identifier reuse or renumbering across revisions because it receives no prior artifact state and checks only duplicates in the current document.

**Exact evidence:**

- `protodog/core/foundation.md:165-167` prohibits reuse or renumbering after execution begins.
- `protodog/core/task-protocol.md:66-69` states that “renumbered IDs are rejected mechanically.”
- `docs/design-rationale.md:36-45` likewise states that ID reuse is mechanically rejected.
- `protodog/validators/validate-plan.cs:60-76` reads only the current path.
- Current-document duplicate checks appear at `protodog/validators/validate-plan.cs:425-451`, `661-674`, and `759-771`; there is no comparison with Git history, a prior version, or persisted identity ledger.
- The validator tests cover duplicate IDs but no between-version renumbering case (`protodog/validators/test-validators.sh:45-49`).

**Affected surface:** Steps, blocks, tracks, gates, acceptance criteria, and deferred-register provenance.

**Concrete consequence:** An executed ID can be renamed or reused while the resulting file remains validator-valid, silently invalidating audit references, decisions, evidence, and resumption pointers.

**Confidence:** High — the validator has no input from which historical stability could be established.

### FINDING-05 — Contradictory terminal state can validate and become sweepable

**Claim:** A plan may declare itself `completed` while retaining pending or unmet acceptance, incomplete work rows, or open gates; neither the validator nor sweep checker rejects that contradiction.

**Exact evidence:**

- Task completion requires settled acceptance, required verification, and no blocking gate at `protodog/core/task-protocol.md:212-219`.
- Program completion imposes corresponding requirements at `protodog/core/program-protocol.md:292-300`.
- Header status validation checks only enum membership at `protodog/validators/validate-plan.cs:356-373`.
- Acceptance and gate validation checks row syntax and local marker/status agreement at `protodog/validators/validate-plan.cs:313-342` and `392-423`; it does not correlate those rows with terminal plan status.
- `protodog/scripts/sweep-check.cs:75-113` treats the plan header’s terminal label, disposition-arrow count, and Git comparison as the mechanical retirement proof. It does not inspect acceptance, gates, or work-row completion.

**Affected surface:** Task and Program completion, acceptance integrity, and retirement safety.

**Concrete consequence:** An internally incomplete plan can be marked terminal, pass validation, be reported mechanically sweepable, and lose its live execution state through retirement.

**Confidence:** High — the required correlations are absent from both enforcement paths.

### FINDING-06 — A disposition arrow is accepted without proving that its destination exists

**Claim:** `→ D-NN` and `→ ISSUE-NN` are treated as proof of obligation lift without checking the referenced deferred-register or Program-ledger row.

**Exact evidence:**

- `protodog/core/foundation.md:124-132` requires every live obligation to lift to the deferred register as a self-sufficient row or record closure.
- `protodog/validators/validate-plan.cs:458-499` validates only an arrow regex and deferral tag; it never resolves the referenced ID.
- `protodog/scripts/sweep-check.cs:39-41` defines lift as a matching arrow, and `protodog/scripts/sweep-check.cs:123-153` counts any top-level item lacking that pattern as unlifted.
- Neither implementation reads `plan/deferred.md` or the Program issue ledger to establish that the destination row exists and carries the obligation.

**Affected surface:** Terminal-plan obligation transfer, deferred register, Program issue ledger, and retirement.

**Concrete consequence:** A typo or fabricated destination such as `→ D-99` can make a live obligation appear lifted; retirement can then remove its sole substantive record.

**Confidence:** High — the implementations perform syntax matching without referential lookup.

### FINDING-07 — The retirement checker’s “committed” proof excludes untracked content

**Claim:** `sweep-check.cs` can report a plan directory as committed even when it contains untracked files absent from the base ref.

**Exact evidence:**

- The plan-sweep contract says the checker proves content is committed and identical to the base ref at `protodog/skills/plan-sweep/SKILL.md:15-20`.
- `protodog/scripts/sweep-check.cs:156-164` first checks that the directory exists in the base tree, then uses `git diff --quiet <base> -- <path>` as the complete committed-state test.
- Standard `git diff` comparison does not include untracked files.
- The checker’s artifact scan at `protodog/scripts/sweep-check.cs:66-68` considers only named plan files for terminal status; other untracked audit or dispatch content does not affect the result.

**Affected surface:** Retirement safety and the claim that Git history is the complete archive.

**Concrete consequence:** A directory containing untracked protocol artifacts can be labeled “terminal, lifted, committed,” even though those artifacts have no historical copy and may remain as residue or be lost during cleanup.

**Confidence:** High — the Git command used cannot establish the claimed property for untracked paths.

### FINDING-08 — Program-level acceptance ownership is not referentially validated

**Claim:** Acceptance IDs in Program track rows and track block rows may reference nonexistent criteria while passing validation.

**Exact evidence:**

- Program readiness requires every applicable criterion to be assigned to a track or Program reconciliation, and each ready track to map its assigned acceptance (`protodog/core/program-protocol.md:54-65`).
- In Program-plan validation, `protodog/validators/validate-plan.cs:661-705` validates track identity, track-plan reference shape, and dependencies but never checks the Acceptance cell at `row[4]`.
- The Program acceptance table is parsed at `protodog/validators/validate-plan.cs:708`, but its returned ID set is discarded.
- Track block validation at `protodog/validators/validate-plan.cs:755-789` similarly ignores the block Acceptance cell at `row[3]`.
- Only detailed step rows are checked against the track acceptance table at `protodog/validators/validate-plan.cs:800-804`.

**Affected surface:** Program acceptance ownership, progressive planning, track closure, and final Program completion.

**Concrete consequence:** A Program can carry an apparently complete ownership map whose IDs do not exist, leaving criteria unowned or allowing work to close without evidence for the intended acceptance condition.

**Confidence:** High — the relevant cells are parsed but never compared with the acceptance-ID sets.

### FINDING-09 — The audit launcher does not preserve the declared freeze window

**Claim:** The guarded launcher verifies hashes only before launch and provides neither locking nor post-capture re-verification, so concurrent writes can change manifested files while the auditor reads them.

**Exact evidence:**

- The assurance contract requires manifested files to remain frozen from launch through report capture at `protodog/core/verification-and-assurance.md:176-185`.
- `protodog/scripts/audit-launch.cs:67-99` verifies manifest hashes before starting the auditor.
- `protodog/scripts/audit-launch.cs:119-159` launches and waits for the process, then captures the report without reacquiring hashes or checking a lock.
- The hook implementation contains no audit-in-flight state or manifested-path freeze mechanism (`protodog/hooks/protodog-hooks.cs:51-187`).
- The neutral operational evidence records this exact moving-target failure class at `inputs/protocol-cost.md:77-80`.

**Affected surface:** Audit target identity, evidence provenance, and reusability of audit findings.

**Concrete consequence:** An audit may read a mixture of pre-verified and subsequently modified content while the report still carries the original dispatch hash and appears pinned to the original target.

**Confidence:** High for the absence of enforcement; cooperative agents may obey the prose rule, but concurrent writers are not mechanically prevented or detected.

### FINDING-10 — A failed auditor process can be captured as a successful immutable report

**Claim:** The launcher accepts any nonempty output file regardless of the auditor process exit code.

**Exact evidence:**

- `protodog/scripts/audit-launch.cs:139-148` waits for the process and refuses only if the output is absent or empty.
- `protodog/scripts/audit-launch.cs:150-158` prepends the provenance stamp, makes the file read-only, logs `DONE`, and prints success; `process.ExitCode` is only included in the log and is never used as a gate.
- `protodog/scripts/test-audit-launch.sh:1-43` exercises dry-run gates only and does not test nonzero auditor termination with partial output.

**Affected surface:** Audit report integrity, audit-cycle immutability, and downstream challenge/remediation.

**Concrete consequence:** A timeout-adjacent failure, CLI error, or partial final message that leaves nonempty output can be stamped and frozen as the authoritative audit report.

**Confidence:** High — the success path lacks an exit-code condition.

## 6. Clean areas

Within the stated coverage:

- Every manifested hash matched, and the repository identified itself as commit `53e9101` on `main` with the expected release 3.8.0 working-tree changes.
- Task and Program vocabulary, profile-selection authority, cadence semantics, gate types, and the distinction between completion and commit to main were consistent across the normative core, entry skills, README, and templates.
- The inspected marker/status maps, settled-gate strikethrough grammar, fenced-block masking, contents-index comparison, and current-document duplicate detection were internally consistent between prose, templates, validator implementation, and their targeted tests.
- Audit selection, launch authorization, external data-egress disclosure, independent challenge ownership, and the prohibition on auditor remediation were consistently stated across the Foundation, assurance policy, profile documents, challenge agent, and README.
- Sequential-default concurrency, isolated concurrent writers, coordinator plan ownership, and serialized reconciliation were consistent across the Foundation, Program and Task profiles, dispatch template, and Git policy.
- The prompt-registry style contract, registry family descriptions, visible prefixes, and protocol-state boundaries were consistent across both registries and their validator.
- The working-canvas and ideation-audit materials consistently treated their outputs as non-authoritative working or ideation state, with explicit promotion required before entry into Protocol context.
