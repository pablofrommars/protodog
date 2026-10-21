# Staged deferred review

Use this procedure only for `triage` or `verify <handoff-path>`. The skill's per-row verification
questions and settlement rules apply in both modes. The stages are directed review, not an
independent audit; a second model must check evidence, but vendor diversity is not required.

## Triage

1. **Capture the baseline.** Inventory every parked D-id in `plan/deferred.md`. Record the
   repository root, branch, full HEAD, date, register SHA-256, and working-tree status. Use tools
   to compute hashes. Note dirty/untracked files so HEAD is never presented as their contents.
   If there is no register or no parked work, report that and stop without creating a handoff.
2. **Bound discovery.** Group rows by subsystem to reuse relevant reads. Use a targeted
   search/read pass per row, following direct code and test references; inspect historical
   plans only when current evidence needs them. A test's source is evidence of its assertion,
   not a passing test run. Stop at broad historical tracing, architectural ambiguity, or an
   expensive reproduction and record the exact question for verification. Honor any supplied
   time/token/row budget; mark untouched rows `not assessed` when it is exhausted. Do not run
   paid evaluations or change application code as part of triage.
3. **Classify with evidence.** Give every parked row exactly one result:

   | Result | Required basis and next step |
   |---|---|
   | supported keep | Current evidence supports the claim and its default/revisit condition; propose keep, subject to verification-stage spot checks. |
   | possible transition | Evidence suggests close, activate, amend, or drop; name the candidate and what must be verified. |
   | needs deeper investigation | State the unresolved technical question and where the next model should look. |
   | needs engineer or external evidence | State the decision or observation the repository cannot establish; include the current default and a recommendation where applicable. |
   | not assessed | No assessment completed; name the budget or other limit. |

   An old row, absent search result, or the row's own assertion is not sufficient closure
   evidence. Unsupported keeps belong in deeper investigation. A mixed row takes the route
   requiring further work; record any external dependency alongside it. Legacy disposition
   wording is preserved in the handoff; uncertain meanings become questions, not silent migration.
4. **Save a handoff.** Use an engineer-supplied destination, otherwise a new
   `inputs/deferred-review/<unique-review-id>/triage.md` (for example a timestamp). Never overwrite
   another review. This is evidence input, not canonical plan state or a second register. Write
   only the handoff; leave `plan/deferred.md` unchanged. Use the compact format below, with one
   section per inventoried row. Include the complete source row so verification can detect drift.
   For cited files, record paths, symbols/lines, and SHA-256 of the bytes inspected, including
   untracked/dirty files; record commands, results, and dates for any checks actually run. Record
   the selected model/effort if known, otherwise `unknown`; never invent a resolved model ID.
5. **Stop at the handoff.** Report counts by result, coverage and budget limits, and the saved
   path. Recommend the capability needed for verification from the unresolved questions. Give
   the exact host invocation: Claude `/protodog:deferred-review verify <path>` or Codex
   `$protodog:deferred-review verify <path>`. The engineer selects the verification model and
   invokes it; do not automatically continue into verification or request transition ratification
   during triage.

### Handoff format

```markdown
# Deferred review handoff

- Review: <unique ID>; assessed: <date>; model/effort: <known selection or unknown>
- Repository: <root>; branch: <branch>; HEAD: <full commit>
- Register: plan/deferred.md; SHA-256: <hash>
- Working tree: <status and relevant dirty/untracked paths>
- Coverage: <all parked IDs>; counts: <by result>; limits: <budget and omissions>

## D-<id>

- Source row: <complete original row>
- Triage: <one result>; candidate: <keep/close/activate/amend/drop or unresolved>
- Evidence: <path, symbol/line, file hash, and what it establishes; or none>
- Checks run: <command, result, date; or none>
- Open question / next step: <precise question, decision, observation, or spot-check eligibility>
- Verification: pending

## Clusters

- Seed: D-<id> — <activation proposal, or the engineer's focus>
- Members: D-<id> — <surface | trigger>: <symbol, dataset, or trigger>; verification: <shared verification and its cost>
- Excluded: D-<id> — <failed axis, or verification veto: <what differs>>
- After: <work a member must follow, or none>
- Untracked: <defect with no row, for the residue report and the adopting Task; or none>
```

The cluster section is written at verify from the grouped reads; triage may leave it empty.

## Verify a handoff

1. **Reconcile before reuse.** Read the handoff and current register. Confirm repository identity;
   a relocated clone is acceptable when history and contents match. Compare HEAD, register hash,
   working-tree status, source rows, and cited file hashes. A changed HEAD requires inspecting
   intervening changes for relevance, not automatic acceptance or rejection of the whole handoff.
   Recheck affected evidence and dependent claims; file hashes alone do not establish that no new
   caller or implementation changes the conclusion. Changed rows need fresh assessment; added
   parked IDs join the queue; rows already closed/dropped are recorded as such and never restored
   from the snapshot. Missing IDs are a reconciliation discrepancy, not evidence of closure.
2. **Complete focused verification.** Check every possible transition against current primary
   evidence before proposing it. Investigate each deeper question and assess every `not assessed`
   row. Seek the named external evidence or engineer decision where required; preserve unresolved
   items explicitly when unavailable. Reuse valid supported-keep evidence and spot-check at least
   three such rows (all if fewer), choosing different subsystems and the weakest evidence first.
   If a spot check contradicts triage, recheck supported keeps sharing the failed assumption or
   subsystem; expand to all supported keeps if the failure is systematic. Unchanged hashes do
   not replace substantive verification of transition candidates or sampled keeps.
3. **Record coverage.** Append verification results to the handoff with the current baseline,
   evidence, remaining questions, and per-row status: `verified`, `triage evidence reused`,
   `unresolved`, or `already reconciled`. Preserve original triage findings so the two stages
   remain distinguishable. Record sampled IDs and any expanded checks. If a budget expires,
   save progress and give the same verify invocation for resumption; reuse still-current completed
   checks. Never describe unresolved or unassessed rows as verified, or claim a full review while
   they remain. Do not repeat settled proposals after resumption.
4. **Settle through the shared procedure.** Present evidence-backed proposals using the skill's
   steps 2–4, each cluster as one request. Include unresolved rows and the coverage distinction in
   the report; present surviving `needs-decision` rows for the engineer's ruling. Apply only ratified transitions to the current
   register, checking for intervening changes first. Record rulings and outcomes in the handoff
   for resumption, while the register remains the authoritative obligation record. A partial
   review may settle verified proposals without implying that its unresolved rows were checked.
