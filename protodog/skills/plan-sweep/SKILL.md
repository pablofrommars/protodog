---
name: plan-sweep
description: Engineer-triggered retirement sweep of the plan/ tree — proves terminal, lifted, and landed with the deterministic checker, reviews citation liveness, and deletes only engineer-confirmed directories. Use after landing, normally on main.
---

# Plan retirement sweep

You are executing the engineer-triggered retirement sweep the Foundation's plan lifecycle
defines. Invocation is the engineer trigger; nothing is deleted before the confirmation step
below. The protocol expects sweeps on `main` after landing (land-then-sweep) — name the current
branch in the report and flag a sweep anywhere else.

## Procedure

1. **Prove.** Run `dotnet ${CLAUDE_PLUGIN_ROOT}/scripts/sweep-check.cs` (in the protocol home
   repository: `protodog/scripts/sweep-check.cs`; pass `--base <ref>` when the landing base is
   not `main`). It classifies every `plan/` entry and proves the mechanically decidable
   retirement conditions: every plan file terminal, every obligation lifted (disposition
   arrows), content landed (identical to the base ref). Do not re-derive these by hand, and do
   not soften a failed proof.
2. **Review citations.** For each mechanically sweepable directory, search the repository for
   references to it and judge liveness: a citation from a live plan, spec, brief, or repository
   instruction blocks the sweep; a citation from another terminal plan, or one that is purely
   historical, does not. Record the judgment per candidate with the citing locations.
3. **Report.** Present the dry-run result before touching anything: sweepable candidates with
   their evidence; blocked entries with the exact blocker (live status, unlifted items,
   branch-only or diverged content, live citation); foreign entries flagged against the
   membership rule. Legacy pre-3.0 plans may fail the lift proof by design — their lift record
   is the register's Provenance column; verify the register actually carries their items and
   present them separately.
4. **Confirm and delete.** Delete exactly the entries the engineer confirms — `git rm -r`, one
   sweep commit whose subject names it a retirement sweep. Never delete the register or a live
   plan; a failed proof is overridden only by the engineer, explicitly, per entry.
5. **Reconcile.** If a transitional index exists (a pre-3.0 `plan/README.md`), update it — or
   delete it when its own self-declared condition has arrived. Report what was swept, what
   remains, and why.
