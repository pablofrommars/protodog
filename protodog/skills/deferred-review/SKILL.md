---
name: deferred-review
description: Review plan/deferred.md against current repository state — find rows closed by later work, fired triggers, and claims that no longer hold — and apply only engineer-ratified transitions. Use when the register may have drifted.
---

# Deferred register review

You are reviewing the deferred register for drift. This is a review, not an audit: per-row
verification is directed by nature and claims no independence. The register's founding defect was
silent closure — items done or obsoleted with nobody reconciling the row — and that is what this
review hunts.

## Procedure

1. **Verify each parked row** against current repository state; fan out subagent verifiers when
   the register is large. Per row: Has later work closed it — the defect fixed, the question
   ruled, the debt paid? Has a trigger-blocked row's trigger fired? Does an actionable claim
   still reproduce as written? Is an engineer's-call row's stated default still the behavior in
   force? Ground every answer in current evidence — code, tests, plans, history — never in the
   row's own text.
2. **Propose transitions.** Per row, one of: **keep** (still true, evidence current); **close**
   (done or obsoleted — with the closing evidence); **activate** (a fired trigger proposes
   starting the work, never silent closure); **amend** (claim, default, or trigger drifted —
   supply the corrected wording). Flag a row as a **drop** candidate only when evidence says the
   work is no longer worth doing; that call is the engineer's. Never apply a transition on your
   own judgment.
3. **Ratify and apply.** Present the proposals with evidence; apply exactly what the engineer
   ratifies — closed and dropped rows move to the Closed table with a one-line outcome and date,
   amendments rewrite the row in place, D-ids are never reused or renumbered. The write is
   validated on save by the register validator.
4. **Report residue.** Close with what was verified and kept, with the evidence that supports
   each survivor, so the next review knows what was already checked and when.
