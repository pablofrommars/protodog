---
name: runbook
description: Cut the current session, or an explicit source, into a dated sequence of Protodog Tasks with standing rulings, execution tiers under the routing ladder, and dispatch prompts for both hosts. Engineer-invoked; a protocol utility, not a Protocol entry.
disable-model-invocation: true
argument-hint: [source or scope narrowing, inherited constraints, host/model preferences]
---

# Runbook

You are cutting work into a runbook: a dated sequence of Tasks the engineer dispatches by hand,
one fresh session per Task, or through `/protodog:coordinate` from one session. The runbook is the
multi-unit shape of the Protocol and it is advisory: a dated cut, not a ledger, re-issued when the
sequence changes, while live state stays in `plan/` and `plan/deferred.md`. Drafting it creates no
Protodog state and plans nothing.

## Arguments

The invocation text, if any, is the context block below: a narrower session scope or another
source, inherited constraints, work already in flight, host or model preferences, or changes
since the last cut. On Claude Code it replaces `$ARGUMENTS`; on Codex it follows the skill
mention. Omitted, the source is the visible session so far.

## Repository state at invocation

Injected on Claude Code before the prompt is read; on Codex, inspect the same facts by hand.

- HEAD: !`git rev-parse --short HEAD 2>/dev/null || true`
- Branch: !`git branch --show-current 2>/dev/null || true`
- Working tree (first lines): !`git status --short 2>/dev/null | head -15 || true`
- Worktrees: !`git worktree list 2>/dev/null || true`

<task>
Cut the work discussed in the current session, or an explicitly supplied source, into a sequence of Protodog Tasks, and return the runbook that dispatches them. The runbook is a dated cut, not a ledger: it is re-issued when the sequence changes, and live state stays in plan/ and plan/deferred.md.
</task>

<context>
$ARGUMENTS
</context>

<instructions>
- Use the visible session by default; no handoff document is required to draft the runbook. Explicit sources may instead be an ideation handoff, a register cluster proposed by deferred review, or an audit reconciliation. State any missing source context rather than reconstructing unavailable turns.
- Reconcile settled conclusions, my later corrections, constraints, rejected directions, assumptions, and unresolved questions. Preserve the latest settled intent; distinguish my decisions from your recommendations and inference. Ground session anchors in descriptive topics or short quotes without inventing turn numbers, source steps, or file references.
- Inspect repository instructions, relevant code, HEAD, branch, and working-tree state, plus applicable plans and plan/deferred.md when present. Date the cut against the inspected state; distinguish uncommitted work and completed Tasks from work proven landed. Do not ask me to transcribe repository facts you can inspect.
- Before writing the sequence, settle the standing rulings in one interrogation — the host's structured question tool where it exists, the decision-ready text form otherwise — asking only what the source has not settled: the landing policy (each unit lands on main as its own commit, or the whole declared line lands as one commit; name the costs: ids allocated on parallel branches can collide, and a long-lived line rebases the register against every landing in between), the cadence of dispatched units, surface grants (`authority:` grants for adjacent fixes the units may absorb), assurance selections, and any paid run. Each ruling is carried into the affected dispatch prompts as the Task skill's plain-language overrides (`cadence:`, `authority:`, `assurance:`), present only where it differs from the default, and listed once in the header as standing rulings. A ruling I defer is listed as open, with the default that holds.
- Ground each unit's prerequisites and exclusive write boundaries in the source and repository. Separately landed units bind against prerequisites on main; dependent units in a declared line bind against the line's branch and record that basis. Parallel units require disjoint write boundaries and isolated writer worktrees inside the repository's writable roots; name their reconciliation and combined-state verification.
- Recommend an execution tier per unit under the Foundation's routing ladder — three rungs, never the small tier for a Task; low confidence that a rung is capable selects the next rung up — naming the rung per host, the confidence, the demand signals behind it, and what would move it. Carry any engineer-specified host/model preferences separately, including named models. These are run-specific preferences, not new routing pins; disclose unavailable or unverified choices rather than silently substituting. The Task's own readiness report and assurance interrogation re-decide suitability and assurance; recommendations authorize no paid or external launch.
- Name objective-level completion against the combined final state under the landing policy: a closing Task over the integrated line, or an external audit over the landed sequence. Isolated unit verification does not prove the combined objective.
- Make each dispatch usable in a fresh session: include its source-grounded intent, acceptance anchors, inherited constraints, standing rulings as overrides, and material preferences. For session sources, carry a compact synthesis with provenance rather than relying on the reader having the conversation; cite existing documents exactly and do not invent a persisted handoff.
- On a re-issue, open with what changed and why, and what the next unit inherits from landed units or completed predecessors on the declared line's branch. Reconcile that basis with repository and plan evidence; do not carry status columns forward by hand. Sweep the deferred items of landed units: route each to the downstream unit owning its surface — adopted there as a step `from <plan-id>` when the item closed as carried, or `from D-NN` when it lifted to the register with that unit as its revisit — and list the rest under parked with a reason. Engineer-owned items — the objective, the landing policy, standing rulings, exclusions — hold across re-issues until I rule otherwise.
- Do not create Protodog state, plan, or implement. Do not persist by default; on my explicit instruction save the runbook under inputs/ with cited provenance and enough source intent for its dispatches, and on a re-issue rewrite that file. Ideation material must be persisted under inputs/ with cited provenance before it enters Protocol planning; drafting an in-session runbook does not perform that step.
</instructions>

<output>
Return the runbook: a header dating the cut against the inspected HEAD and naming the source, landing policy, standing rulings with open ones and their defaults, and inherited host/model preferences; the objective-level completion; a sequence table with unit, source anchors (steps, rows, or session evidence), prerequisites on main or the declared line's branch, can run in parallel with, execution tier with confidence, paid cost (estimate or unknown), and why here; then one section per unit with proposed label and plan id, scope, out of scope, exclusive write boundaries, acceptance anchors in the source, required verification, size, execution tier, host/model preferences where supplied, assurance, stopping conditions, routed deferred items, and the self-contained dispatch prompt in both host forms — `/protodog:task <label>. … scope: …. authority: …. cadence: ….` for Claude Code and the same text after `$protodog:task` for Codex. State assurance recommendations beside the dispatch; include an `assurance:` override only for an already selected choice. End with parked and excluded items with reasons and, on a re-issue, what the next cut inherits.
</output>
