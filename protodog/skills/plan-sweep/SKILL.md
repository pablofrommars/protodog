---
name: plan-sweep
description: Engineer-triggered retirement sweep of the plan/ tree — proves terminal, lifted, and committed with the deterministic checker, reviews citation liveness, and deletes only engineer-confirmed directories. Use after the commit to main, normally on main.
---

# Plan retirement sweep

You are executing the engineer-triggered retirement sweep the Foundation's plan lifecycle
defines. Invocation is the engineer trigger; nothing is deleted before the confirmation step
below. The protocol expects sweeps on `main` after the commit to main (commit-then-sweep) — name
the current branch in the report and flag a sweep anywhere else.

## Procedure

Resolve the package root as `../..` from this `SKILL.md`'s directory; read
`core/host-runtime.md` there for host mechanics. Explicit instructions such as `no commits`
remain in force throughout the sweep.

1. **Prove.** Run `dotnet <package-root>/scripts/sweep-check.cs` (in the protocol home
   repository: `protodog/scripts/sweep-check.cs`; pass `--base <ref>` when the commit base is
   not `main`). It classifies every `plan/` entry and proves the mechanically decidable
   retirement conditions: every plan file terminal, every obligation lifted (disposition
   arrows), content committed (identical to the base ref). It also classifies every file under
   `specs/` and `inputs/`: live (cited by a live plan or spec), retiring (cited only by a
   sweepable directory, so it goes in the same sweep), retirable (cited by nothing, consumed by a
   plan in history), backlog (cited by nothing, never consumed — work shaped and not started,
   never a sweep candidate), or blocked (follows a blocked plan, or not committed, so nothing
   would archive it). A `docs/` citation into `inputs/` or `specs/` is reported as a defect:
   documentation is self-contained and cites history, not working-tree material. Do not
   re-derive these by hand, and do not soften a failed proof.
2. **Review citations.** For each mechanically sweepable directory, search the repository for
   references to it and judge liveness: a citation from a live plan, spec, brief, or repository
   instruction blocks the sweep; a citation from another terminal plan, or one that is purely
   historical, does not. Record the judgment per candidate with the citing locations.
3. **Report.** Present the dry-run result before touching anything: sweepable candidates with
   their evidence; retiring and retirable specs and inputs beside them; blocked entries with the
   exact blocker (live status, unlifted items, branch-only or diverged content, live citation);
   foreign entries flagged against the membership rule; backlog inputs listed as the backlog,
   not as candidates. With the register's parked rows, this report is the backlog view. Legacy pre-3.0 plans may fail the lift proof by design — their lift record
   is the register's Provenance column; verify the register actually carries their items and
   present them separately.
4. **Confirm and delete.** Delete exactly the entries the engineer confirms — `git rm -r`
   for directories, `git rm` for retiring and retirable files — in one sweep commit whose subject
   names it a retirement sweep. Never delete the register, a live plan, a live or backlog file,
   or anything not committed; a failed proof is overridden only by the engineer, explicitly, per
   entry.
5. **Reconcile.** If a transitional index exists (a pre-3.0 `plan/README.md`), update it — or
   delete it when its own self-declared condition has arrived. Report what was swept, what
   remains, and why.
