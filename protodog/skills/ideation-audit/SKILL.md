---
name: ideation-audit
description: Dispatch independent adversarial audits of the current ideation session's settled conclusions — grounded against the repository, purely conceptual, or both — so design concepts and logic are settled before planning. Use from inside an ideation mini protocol.
---

# Ideation audit dispatch

You are dispatching adversarial audits of the current ideation session's design conclusions. The
purpose is to settle important design concepts and logic *before* planning: conclusions leave
ideation as `inputs/` documents, and this skill applies independent adversarial pressure while
they are still cheap to change. This is Claude-native mechanics accompanying the host-portable
ideation mini protocols (`p-protocol-ideate-*`); it is not a Protocol entry point, creates no
Protodog state, and never satisfies Protocol assurance — the numbered audit cycle with the
external auditor is a separate, unrelated contract. The portable fallback without this skill is
`p-audit` in a fresh session.

## Arguments

Stated in plain language inside the invocation, only when they differ from defaults:

- `mode: grounded | conceptual | both` — default `both` when repository surfaces materially
  support the conclusions, otherwise `conceptual`. This tracks the mini protocol naturally:
  feature sessions usually warrant `both`, concept sessions `conceptual`, domain sessions `both`
  exactly when repository references entered the session.
- `scope: <subset>` — bound the audit to named conclusions or decision surfaces; default is every
  settled conclusion plus the material assumptions beneath them. A scope narrows what the
  auditors receive, never what they should suspect.

## Procedure

1. **Distill the brief.** Condense the session ledger — settled conclusions, active directions,
   material assumptions, and the constraints they answer to — into a self-contained brief with
   claim provenance (`engineer stated / source cited / inferred`). The brief carries exactly
   three things: the claims under audit, stated neutrally; the auditor's access and boundaries;
   and the reporting convention below. It must be executable without the session transcript.
   Exclude session narrative, the reasoning that produced a conclusion (unless that reasoning is
   itself a claim under audit), and any steering — no suspected weaknesses, no priority hints, no
   per-claim instructions. Within its scope the audit is undirected: the auditors choose where to
   dig. A directed check ("verify function X against Y") is ordinary session work, not this
   dispatch. A working canvas maintained by the session informs this distillation but is never a
   substitute for it or attached to it — the brief stays self-contained.
2. **Dispatch fresh-context subagents**, one per selected mode:
   - **Grounded auditor** — receives the brief and repository read access. Attacks the
     conclusions' factual footing: does the repository actually behave as claimed, do the cited
     surfaces exist and mean what the conclusion assumes, and which existing seams, constraints,
     or invariants does the design contradict or ignore?
   - **Conceptual auditor** — receives the brief only, explicitly without repository access, so
     it cannot anchor on implementation detail. Attacks the logic: unstated assumptions, internal
     contradictions, the weakest link in each chain of reasoning, materially distinct alternatives
     dismissed without evidence, and second-order consequences the conclusions do not price in.

   Each auditor is adversarial toward claims but symmetric about the result. Its mode names a
   lens, not a checklist: it considers those angles where applicable, follows the evidence it
   actually finds, and does not remediate, redesign, or plan.
3. **Reporting convention.** Each auditor returns atomic falsifiable findings — claim, exact evidence
   (a repository reference or the brief's own text), affected conclusion, concrete consequence,
   and `high / medium / low` confidence — plus stated coverage and limitations. No findings are
   invented to fill a report shape; a clean result applies only to the stated coverage.
4. **Reconcile in session.** Merge the findings into the session as challenge material: update the
   `settled conclusions / active directions / unresolved decisions` ledger, reopening any
   conclusion a finding materially undermines. When the session maintains a working canvas,
   reconciliation includes it: a reopened conclusion recolors its nodes red. Adopting, rejecting,
   or testing a finding is session work with the engineer; a finding is never silently
   authoritative.
5. **Persistence.** Nothing persists by default. On explicit instruction, results destined for
   planning are saved under `inputs/` with cited provenance, like every ideation product.
