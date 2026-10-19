<!-- launched by audit-launch.cs: dispatch audit-01-dispatch.md sha256=883a421588146d4cecc28d04d4426f292999e2e291b4f96db4effb07b3969c22 model=gpt-5.6-sol sandbox=read-only -->
# audit-01 — Independent adversarial audit of the Protodog protocol

## Scope and target state

This audit covers the single declared dimension **unnecessary complexity, ceremony, or duplication** across the complete manifested Protodog protocol, including cross-artifact consistency where needed to establish a finding.

The target state is repository commit `53e9101` on `main` plus the uncommitted release 3.8.0 working-tree changes identified by the dispatch manifest. I independently recomputed every manifested SHA-256 hash; all matched. The repository reported `HEAD` `53e9101` on branch `main`, with the expected modified and untracked working-tree surfaces.

## Sources used

I read every manifested file.

### Normative core

- `protodog/core/foundation.md`
- `protodog/core/git-policy.md`
- `protodog/core/program-protocol.md`
- `protodog/core/task-protocol.md`
- `protodog/core/verification-and-assurance.md`

### Enforcement

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

### Entry points and mechanics

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

### Host-portable prompt registries

- `protodog/registries/boxer.code-snippets`
- `protodog/registries/prompt-registry-style.md`
- `protodog/registries/prompt-utility.code-snippets`

### Operations and rationale

- `README.md`
- `docs/design-rationale.md`

### Neutral references

- `CHANGELOG.md`
- `inputs/protocol-cost.md`
- `inputs/ideation-visual-support-handoff.md`

## Coverage

I statically inspected all 6,065 manifested lines, traced the lifecycle from entry skills through plans, enforcement, audit, completion, deferred-state handling, and retirement, and cross-referenced normative claims against templates, hooks, validators, tests, registries, and maintenance consumers. Material methods included manifest verification, line-by-line inspection, source searches for duplicated concepts and consumers, and control-flow review of the C# and shell enforcement.

No manifested file was inaccessible. I did not run the test suites because the dispatch prohibits creating or modifying files, while the suites create temporary fixtures and the hook suite may append a repository-local denial log. This is therefore a static audit; runtime behavior and performance were not measured.

The other standard dimensions were not audited independently. Consistency, enforcement, maintainability, and operability were considered only where they evidenced unnecessary ceremony or duplication. The absent `plan/` directory was treated as instructed and is not a finding.

## Overall assessment

Within the inspected dimension, Protodog has a coherent lean core around authority boundaries, acceptance evidence, bounded dispatch, audit independence, and Task/Program proportionality. Its largest avoidable costs are concentrated in the plan dialect and lifecycle: derived visual state is stored beside authoritative state and then mechanically synchronized; plans duplicate their own structure; every incremental edit pays whole-artifact validation; deferred work is classified twice and closed work is retained despite an explicit no-archive rule.

Several newer features also add mandatory state without a canonical artifact contract, while one advertised validator guarantee—stable identifier preservation—cannot be established by the validator at all. The result is not uniformly overengineered, but the protocol does not meet its own “never for ceremony or symmetry” standard on the findings below.

## Findings

### FINDING-01 — The deferred register is required to retain a historical archive that the plan lifecycle says must live in Git

**Claim:** The mandatory `Closed` table preserves dead deferred rows indefinitely inside `plan/`, duplicating Git history and contradicting the rule that `plan/` is live execution state rather than an archive.

**Exact evidence:**

- `protodog/core/foundation.md:118-122` says "`plan/` is working execution state — never an archive."
- `protodog/core/foundation.md:152-157` says Git history on `main` is the archive and retirement removes terminal plan directories.
- `protodog/templates/deferred-register.md:18-22` defines a permanent `## Closed` table.
- `protodog/validators/validate-plan.cs:553-587` rejects a register without that table and validates retained closed/dropped rows.
- `protodog/skills/deferred-review/SKILL.md:17-22` verifies parked rows, while lines 29-32 move closed and dropped rows into `Closed`; no later decision procedure consumes those rows.
- `protodog/scripts/sweep-check.cs:53-57` merely prints their count.

**Affected surface:** Deferred-register lifecycle, register validation, deferred review, and `plan/` retirement semantics.

**Concrete consequence:** The only non-retired plan artifact accumulates historical rows that do not affect execution or future review, increasing scan and maintenance cost while duplicating the repository history designated as the archive.

**Confidence:** **High** — the retention rule, mandatory validation, and absence of an operational consumer beyond a count are explicit.

### FINDING-02 — A lifted deferral must be classified under two overlapping taxonomies

**Claim:** The protocol requires both a `defer:` reason tag and a separate deferred-register disposition for the same obligation, with no defined mapping between them.

**Exact evidence:**

- `protodog/core/foundation.md:144-148` defines `defer: out-of-scope | needs-decision | cost | blocked-by` and requires a lifted item to carry the failed-condition tag.
- `protodog/validators/validate-plan.cs:454-499` enforces that taxonomy on terminal-plan lifts.
- `protodog/validators/validate-plan.cs:45` separately defines register dispositions as `actionable | engineer's call | trigger-blocked | standing debt`; lines 545-548 enforce them.
- `protodog/templates/deferred-register.md:14-16` gives every parked row a mandatory `Disposition` cell in addition to its item text.
- `protodog/core/program-protocol.md:266-280` carries the `defer:` classification through the Program issue ledger before a later lift to the register.

**Affected surface:** Finding triage, Task and Program closure, Program issue ledgers, the deferred register, and their validators.

**Concrete consequence:** Agents must translate one deferral into two classifications whose meanings overlap but are not equivalent; contradictory combinations such as a `defer: blocked-by` item marked `actionable` are structurally accepted, while every lift pays duplicate classification effort.

**Confidence:** **High** — both required enums and the lack of a cross-check or mapping are directly visible in the normative text and validator.

### FINDING-03 — The Program template creates an optional acceptance section that the Program protocol says to omit by default

**Claim:** Every new Program scaffold includes `Program acceptance`, although the canonical kernel and omission rule say that section exists only for cross-track reconciliation criteria.

**Exact evidence:**

- `protodog/core/program-protocol.md:94-97` says the Program kernel carries only `## Tracks`.
- `protodog/core/program-protocol.md:105-109` says to add `Program acceptance` only for criteria requiring cross-track reconciliation.
- `protodog/core/foundation.md:169-172` requires absent optional sections to be omitted.
- `protodog/templates/program-plan.md:10-25` includes both the contents entry and a placeholder `Program acceptance` section in every scaffold.
- `protodog/skills/program/SKILL.md:25-35` creates Programs from that template before acceptance topology is resolved.

**Affected surface:** Program creation, program-plan maintenance, contents indexes, and acceptance ownership.

**Concrete consequence:** A Program without cross-track acceptance must immediately delete the scaffolded section and its contents entry, or retain redundant state contrary to the protocol; the canonical creation path therefore manufactures cleanup work.

**Confidence:** **High** — the template directly contradicts the profile’s explicit optional-section rule.

### FINDING-04 — Plan rows maintain redundant status encodings, and settled gates encode the same state three times

**Claim:** The marker, status label, and gate strikethrough are synchronized representations of state rather than independent information.

**Exact evidence:**

- `protodog/core/foundation.md:176-201` requires every state row to contain both a marker and exact status label and requires them to agree.
- `protodog/core/foundation.md:210-212` adds mandatory strikethrough for a settled gate, producing forms such as `✅ ~~GATE-01~~ · settled`.
- `protodog/validators/validate-plan.cs:12-49` defines the derived marker maps; lines 235-270 validate marker/status agreement and strikethrough.
- `protodog/validators/test-validators.sh:61-137` contains numerous tests solely for marker/label combinations, and lines 187-194 add gate-strikethrough combinations.
- The observed cost record reports a real rejection caused only by using `🔵` instead of the marker enum (`inputs/protocol-cost.md:31-52`).

**Affected surface:** Every step, block, track, acceptance, gate, and deferred-register row; plan validation and its tests.

**Concrete consequence:** Every state transition must update multiple correlated presentation signals, and non-semantic disagreement blocks work while requiring substantial validator and test machinery. Settled gates are especially duplicative: `✅`, `~~…~~`, and `settled` all encode closure.

**Confidence:** **High** — the duplication and its observed clerical rejection are explicit.

### FINDING-05 — Every plan duplicates its section structure in a mandatory contents index

**Claim:** `## Contents` repeats all `##` headings and must be synchronized after every section addition, removal, or reorder, including in minimal two-section Task plans.

**Exact evidence:**

- `protodog/core/foundation.md:217-220` requires every plan to list every other section in document order.
- `protodog/core/task-protocol.md:99-104` requires updating the index whenever a section changes.
- `protodog/core/task-protocol.md:256-289` shows that even the minimum one-step Task must carry a two-entry contents index.
- `protodog/validators/validate-plan.cs:273-310` reparses and compares the duplicate list.
- `protodog/validators/test-validators.sh:94-106` maintains four negative cases for this synchronization rule.

**Affected surface:** All Task, Program, track, and deferred-register documents and every structural edit to them.

**Concrete consequence:** The same structure is authored twice, creating edit and validation work without adding execution state; the cost is paid even where the complete document is shorter than a useful navigation index.

**Confidence:** **Medium** — duplication and cost are certain, while the protocol’s stated scanability benefit may be useful in unusually large plans.

### FINDING-06 — Whole-artifact validation is a blocking operation after every incremental plan edit

**Claim:** Validation cadence is tied to individual `Write` or `Edit` tool calls rather than completed semantic transitions, so temporary or purely clerical intermediate states invoke and may fail the full validator.

**Exact evidence:**

- `protodog/hooks/hooks.json:13-17` attaches `post-plan-write` to every `Write|Edit`.
- `protodog/hooks/protodog-hooks.cs:162-186` starts a new `dotnet` validator process and denies the completed tool call on any validation error.
- `protodog/core/foundation.md:322-330` already requires verified persisted state before dependent work under continuous cadence, providing a semantic transition boundary distinct from each editor operation.
- The observed cost record calls validation after every edit “the lowest” value and “nearly all clerical,” reporting three shape-only interruptions (`inputs/protocol-cost.md:31-53`).
- `docs/design-rationale.md:174-177` records that the every-write cadence was deliberately retained despite that evidence.

**Affected surface:** All plan, dispatch, and deferred-register editing through the normal plugin tools.

**Concrete consequence:** Multi-edit updates are serialized through repeated process startup and whole-file parsing, and harmless intermediate formatting states interrupt work even when no dependent execution has begun.

**Confidence:** **High** — trigger granularity, process behavior, and observed cost all align.

### FINDING-07 — The claimed mechanical protection against identifier renumbering cannot fire

**Claim:** The validator checks only the current artifact, so it can reject malformed or duplicate IDs but cannot determine whether a valid ID was reused or renumbered from a prior state.

**Exact evidence:**

- `protodog/core/foundation.md:165-167` prohibits reuse or renumbering after execution begins.
- `protodog/core/task-protocol.md:66-69` says “renumbered IDs are rejected mechanically.”
- `docs/design-rationale.md:36-39` likewise claims that “ID reuse” is mechanically rejected.
- `protodog/validators/validate-plan.cs:425-449` checks current-format regexes, current-file uniqueness, and current acceptance references only.
- `protodog/validators/validate-plan.cs:926-953` walks only the current directory contents and loads no baseline or prior revision.
- `protodog/validators/test-validators.sh:45-48` tests a duplicate current ID, not historical renumbering or reuse.

**Affected surface:** Stable identifiers, plan links, resumption, audit references, validator claims, and maintenance expectations.

**Concrete consequence:** Maintainers pay for a stable-ID rule while receiving false mechanical assurance: changing `STEP-01` to another syntactically valid unused ID passes the implemented checks and can silently invalidate external references.

**Confidence:** **High** — historical renumbering is unobservable to the validator’s inputs and code path.

### FINDING-08 — The assurance interrogation mandates repeated negative state without defining its canonical form or enforcement

**Claim:** Every planning scope must persist an explicit outcome for both audits and paid/external evaluation runs—even when neither is indicated—but the templates and validator define no field or section contract for those mandatory outcomes.

**Exact evidence:**

- `protodog/core/verification-and-assurance.md:75-89` requires exactly one persisted outcome per instrument, including an explicit not-scheduled decision.
- `protodog/core/program-protocol.md:314-319` repeats this at Program-wide planning and each track’s planning.
- `protodog/skills/task/SKILL.md:28-31,43-46` and `protodog/skills/program/SKILL.md:30-35` make the interrogation part of every entry path.
- None of `protodog/templates/task-plan.md`, `program-plan.md`, or `track-plan.md` provides an assurance-scheduling field or section.
- `protodog/validators/validate-plan.cs:590-814` validates the three plan types without checking that either instrument has a scheduling outcome.

**Affected surface:** Task planning, Program planning, every track-planning pass, resumption, and execution readiness.

**Concrete consequence:** Routine work accumulates repetitive “not scheduled” decisions, while different agents may persist them in different optional prose forms or omit them without detection; the protocol therefore pays mandatory-state ceremony without obtaining the determinism claimed for mandatory state.

**Confidence:** **Medium** — the missing artifact and enforcement contract is certain, while the rationale records an engineer preference for explicit negatives.

### FINDING-09 — The working canvas generalizes one ideation-session benefit into a parallel planning and execution state view

**Claim:** Planning and execution canvases mirror plan structure, gate state, and completed work into an untracked artifact that cannot support resumption and must be manually kept synchronized with canonical state.

**Exact evidence:**

- `protodog/core/foundation.md:159-167` says the canvas is a non-authoritative working view mirrored only from canonical state.
- `protodog/skills/working-canvas/SKILL.md:23-29` places it in an untracked session-scoped file.
- `protodog/skills/working-canvas/SKILL.md:51-62` requires a planning DAG and execution model, including gate and step-completion recoloring.
- The portable registry repeats the full canvas maintenance contract in all three ideation prompts: `protodog/registries/prompt-utility.code-snippets:28-29`, `60-61`, and `101-102`.
- The cited handoff’s actual evidence concerns ideation (`inputs/ideation-visual-support-handoff.md:27-75`); its planning/execution extension is introduced as “foreseen” at lines 130-158, including the unevidenced assertion that recoloring is “at no extra cost.”

**Affected surface:** Diagram-suited ideation, Task and Program planning, gate presentation, execution progress, skills, and portable prompts.

**Concrete consequence:** Sessions maintain a second representation of topology and progress that is discarded by default and cannot resume the work, adding redraw and drift risk at each gate or completion transition. The planning/execution cost is broader than the evidence supporting the original ideation use.

**Confidence:** **Medium** — the duplicate state and maintenance rules are explicit; the net value depends on the engineer’s visual workflow.

### FINDING-10 — The Task readiness handoff contract is specified in three normative-facing layers

**Claim:** The Task skill says it carries only invocation mechanics but restates the detailed readiness/model-handoff contract already defined in both Foundation and Task protocol.

**Exact evidence:**

- `protodog/skills/task/SKILL.md:8-14` says the core documents are authority and “this skill only carries invocation mechanics.”
- `protodog/core/foundation.md:256-277` defines the model-routing rule, readiness boundary, recommendation contents, and fresh-session handoff.
- `protodog/core/task-protocol.md:55-62` restates the recommendation fields, invocation shape, and `go` behavior.
- `protodog/skills/task/SKILL.md:47-52` restates those details again.
- `CHANGELOG.md:47-49` explicitly records that Task prose and the Task entry skill restate the contract.

**Affected surface:** Foundation, Task profile, Task skill, release maintenance, and future model-routing changes.

**Concrete consequence:** A single behavioral contract must be updated consistently in three places despite one declared authority, increasing review surface and drift risk without adding a distinct consumer-specific semantic.

**Confidence:** **High** — the duplication and the skill’s stated layering boundary are explicit.

## Clean areas

Within the inspected complexity dimension:

- Task remains structurally proportional: it does not require Program tracks, blocks, dispatch files, or issue ledgers (`protodog/core/task-protocol.md:12-17`).
- Program dispatch files are conditional rather than universal; bounded native subagent calls require no persisted dispatch (`protodog/core/program-protocol.md:189-219`).
- The audit cycle’s three artifacts have distinct consumers: dispatch drives the guarded launcher, report drives challenge, and challenge drives ordinary plan work. I found no unused audit artifact in that chain.
- The audit launcher’s manifest verification, read-only sandbox, fixed output path, and no-overwrite rule each enforce a distinct target-integrity or authority boundary; their checks are not merely duplicate prose.
- The separation between `sweep-check.cs` and `plan-sweep` is justified: the script proves mechanically decidable retirement conditions, while the skill performs citation-liveness judgment and engineer confirmation.
- The duplicate hook wiring in `hooks.json` and `settings-fragment.json` has an explicit separate consumer: plugin installation versus direct vendoring fallback.
- Registry prompt self-containment justifies some repeated safety text because `prompt-registry-style.md:47-51` forbids one snippet from depending on another, and Boxer is explicitly preserved as a standalone verbatim family.
- I found no mandatory queues, event logs, attempt logs, branch ledgers, checkpoint tables, completion reports, or separate assurance-plan artifacts beyond the exceptions identified above.