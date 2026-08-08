# Git Policy

## Why this document exists

Git conflicts have been among the most expensive failure modes in agent-assisted engineering. This
policy makes conflict-risk reduction a first-class constraint while making checkpoint commits a
normal, delegated part of execution. It supersedes `bundle/git-worktree-and-integration.md`: the
landing invariants are unchanged; what changed is that execution-phase Git inside a managed
worktree is now the agent's job, with the boundary enforced by hooks rather than described in
prose.

Status: committed shared policy, Claude-native line.

## Authority and the enforced boundary

Repository instructions remain authoritative for exact commands and permitted actors. This policy
narrows Protocol behavior; capability (an available command, a host permission) never establishes
authority.

**Agent-owned, inside the managed worktree** (the engineer-supplied Task/Program worktree or a
provisioned isolated writer worktree): status, diff, and history inspection; working-tree edits;
staging; checkpoint commits; internal branches and internal merges; conflict detection. This
delegation is part of profile invocation and is cadence-independent — cadence governs when the
engineer is consulted, not who commits.

**Engineer-owned, always**: fetch and synchronization with `main`; publishing any ref; rebase onto
`main`; history rewrites; squash; landing; worktree cleanup and branch deletion. Where guarded
repository scripts exist, they are the only path for these operations; where they do not, these
operations happen only by the engineer's hand.

The hook layer enforces this split mechanically — engineer-owned operations attempted by the agent
are denied and logged. The denial log is protocol telemetry: spikes after a model update signal
fit regression; a hook that never fires is a candidate for deletion.

## Committed invariants

1. A Task or Program landing branch reconciles with `main` by rebasing onto it, never by merging
   `main` into it. Ordinary branches and merges remain valid inside the Task or Program.
2. One Task or one whole Program is one landing unit and appears on `main` as one commit. Program
   tracks do not land independently.
3. Final landing is engineer-triggered. A guarded script performs the authorized mechanics after
   that trigger; an agent never infers landing from Protocol completion.
4. Concurrent repository writers never share a worktree. Each writer gets its own isolated
   worktree; provisioning follows repository authority with a defined reconciliation path.
5. The initial profile worktree is supplied by the engineer and may be assumed; entry inspects and
   reconciles it without provisioning ceremony.

Completion, landing, shipping, and deployment remain distinct.

## Checkpoint commits

The agent commits coherent verified units in its managed worktree. Before a checkpoint: inspect the
full worktree and index; reconcile unexpected or intervening changes; stage only the coherent
in-scope unit; run the verification that makes the unit useful as a checkpoint; use a concise
provisional subject describing the verified result. Do not commit merely because a step ended,
split one coherent change to mimic plan granularity, or hide unresolved failure in a nominal
checkpoint.

Checkpoints are provisional history: the engineer reviews the final diff and title at landing, and
squashing rewrites them — preserve a reported recovery reference before any rewrite. A checkpoint
is recorded in the owning plan only when its identity matters for resumption, integration, audit,
or recovery; prefer an exact SHA as repository-state provenance. Plans never copy Git status, and
no Git event log, branch ledger, or checkpoint table exists.

## Repository-state handling

At entry or resumption, inspect the current branch, `HEAD`, worktree status, and applicable base
without changing them. Treat the current worktree as the intended profile worktree unless evidence
contradicts the invocation. Unexpected branch identity, unresolved operations, or unrelated dirty
state is reconciled before affected writes; a routine clean worktree needs no confirmation. When
evidence targets uncommitted state, identify `HEAD` plus the exact changed and untracked state
needed to reproduce the claim.

## Topology

**Task.** The coordinating agent is the only writer in the Task worktree. Subagents inspect,
reason, and return findings without Git topology; when concurrent Task work genuinely needs
multiple writers, each gets an isolated worktree via a handoff identifying branch, worktree,
baseline, write surfaces, verification, and return path. Returned results reconcile into the Task
branch one at a time; affected verification runs against each combined state.

**Program.** The engineer-supplied Program worktree and branch are the coordination state and
landing unit; the coordinating agent is its only writer and never transfers that role. Sequential
track work changes it directly. Prefer concurrent read-only or non-interfering work over concurrent
writers; when writers are needed, the execution topology and dispatch record assignment, baseline,
worktree and branch identity, disjoint write surfaces, required child verification, and the
reconciliation owner and order.

**Serialized reconciliation** admits concurrent child results one at a time: freeze and verify the
returned child state; select the next child by dependency and interference evidence; record source
and target states; integrate with the authorized internal strategy (rebase, merge, and merge
commits all permitted inside the profile); on conflict, preserve or abort per repository policy and
report before attempting another child; rerun affected verification on the updated state; stop
admitting children if it fails and triage; perform integration reconciliation; use the verified
result as the next child's baseline. Two children are never integrated as though the target had not
moved. If isolated writers cannot be provisioned and reconciled predictably, concurrent writers are
unavailable; concurrent non-writing assignments remain possible.

## Refresh, conflicts, and landing

Refresh at coherent clean states often enough that divergence is discovered before it contaminates
dependent work — especially before an audit or evaluation tied to pre-landing state and before
landing; no fixed cadence is imposed. A conflict is an integration failure: preserve or abort per
repository policy, report the exact states and files, and re-evaluate independence, ordering, and
verification. Never weaken acceptance to make a conflict resolve.

```text
verified profile state
  → engineer triggers the guarded landing workflow
  → rebase onto fetched origin/main
  → rerun affected final-state verification
  → squash all profile commits to one
  → apply the workflow's established review/title interaction
  → ordinary fast-forward push of one commit to main
```

If `main` moves after the fetch, the push fails: fetch, inspect, repeat the rebase and affected
verification; never force the `main` update.

## Audit provenance

An audit dispatch identifies the exact plan/spec manifest and repository state it audits — prefer a
checkpoint SHA, which delegated checkpoints make normally available; for uncommitted state, include
`HEAD` plus content identity for changed and untracked surfaces. An audit against a child branch
does not establish the combined state; after integration, audit claims are reused only when the
assurance policy says the state change leaves them valid.

## Firm-repository adaptation

Where a firm repository provides guarded Git scripts and tasks, they are the adapter: they own the
engineer-owned operations, and this policy's delegated window must be reflected in repository
authority, host permissions, guarded scripts, and their behavioral tests as one coordinated change
— worktrees provisioned from a Program baseline, recovery states preserved, serialized child
integration, engineer-owned one-commit landing. Do not work around a repository that has not yet
enabled delegation: no landing tracks separately, no children from stale baselines, no unguarded
commands where guarded ones are mandated. Existing exposed tasks are a strict compatibility
boundary — a new Protocol capability uses a new guarded command, or an existing one changes only
through a reviewed, engineer-approved workflow change. Where a read-only repository-state
diagnostic exists, it is the adapter for state handoff; do not create a competing format.
