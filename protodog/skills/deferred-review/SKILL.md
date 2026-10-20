---
name: deferred-review
description: Review plan/deferred.md against current repository state, optionally triaging first and resuming verification from a saved handoff. Find closed items, fired triggers, and drifted claims; apply only engineer-ratified transitions. Use when the register may have drifted, including when a completed profile offered this review.
---

# Deferred register review

You are reviewing the deferred register for drift. This is a review, not an audit: per-row
verification is directed by nature and claims no independence. The register's founding defect was
silent closure — items done or obsoleted with nobody reconciling the row — and that is what this
review hunts. Profile completion offers this review against just-finished work but never runs it
unprompted; when the offer is taken, start from the rows that work plausibly closed, unblocked,
or amended — a fresh session serves the review better than a degraded closing context.

## Entry and modes

Resolve the package root as `../..` from this skill's directory; read `core/host-runtime.md`
there for host mechanics, including the structured-question fallback. Paths below refer to the
repository being reviewed, not the installed package.

- **No mode:** run the full procedure below, as before.
- **`triage`:** read [the staged review procedure](references/staged-review.md). Make a bounded
  evidence pass and save a handoff; leave the register unchanged and stop at the model handoff.
- **`verify <handoff-path>`:** read the same reference, reconcile the handoff with current state,
  and complete focused verification before entering steps 2–4 below.

The engineer selects the model in each host before invoking a stage. Neither mode switches
models, launches another vendor's CLI, or requires a particular model family. A fresh session
can resume from the handoff without the triage conversation. Triage labels are review metadata,
not new register dispositions. Review authorizes investigation and register upkeep, not fixing
the parked work; activation remains a proposal to start it.

## Full review and shared settlement

1. **Verify each parked row** against current repository state; fan out subagent verifiers when
   the register is large. Per row: Has later work closed it — the defect fixed, the question
   ruled, the debt paid? Has a `blocked-by` row's trigger fired? Does an `out-of-scope` or
   `cost` claim still reproduce as written? Is a `needs-decision` row's stated default still the
   behavior in force? Does an `accepted gap` still hold its reopening condition? Ground every answer in current evidence — code, tests, plans, history — never in the
   row's own text.
2. **Propose transitions.** Per row, one of: **keep** (still true, evidence current); **close**
   (done or obsoleted — with the closing evidence); **activate** (a fired trigger proposes
   starting the work, never silent closure); **amend** (claim, default, or trigger drifted —
   supply the corrected wording). Flag a row as a **drop** candidate only when evidence says the
   work is no longer worth doing; that call is the engineer's. Never apply a transition on your
   own judgment.
3. **Ratify and apply.** Present the proposals with evidence, and present every surviving
   `needs-decision` row as a decision interrogation — the host's structured question tool when
   available (otherwise the conversation), recommendation
   and current default per row — so the engineer can rule it there; apply exactly what the
   engineer ratifies — closed and dropped rows move to the Closed table with a one-line outcome
   and date, amendments rewrite the row in place, D-ids are never reused or renumbered. The write is
   validated on save by the register validator.
4. **Report residue.** Close with what was verified and kept, with the evidence that supports
   each survivor, so the next review knows what was already checked and when. Name any row later
   work closed at trivial cost — evidence the fix-now test was applied too tightly at its source.
