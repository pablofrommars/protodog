# Protocol cost — one task's evidence

Feedback from executing `deployed-study-execution` under Protodog Task 3.5.0: eighteen steps, two
audit cycles, one paid deployed run. Numbers are from this task, not general.

## Mutation proof on guards — the highest-yield element, keep it

**What it costs.** Every new guard runs edit → test → back up the source → mutate it to the exact
defect → confirm red → restore → confirm green. That is 4–5 tool calls and one to three minutes per
guard, depending on which suite has to run. Roughly ten guards this task, so on the order of an hour.

**What it bought, twice, and nothing else could have.**

The stall found by the paid run — a writer and a reader of the same cell disagreeing on its
fingerprint — got a guard. The guard called the same expression twice and asserted equality. A
tautology. It passed against the *unfixed* code, and the suite was green with it in place.

Its replacement asserted that no unit runner calls `StudyFingerprints.For` directly. Also green. Also
worthless in a way I did not see: it proved *absence* of direct calls but never *presence* of the
wrapper call, so deleting every call site in `StudyUnitRunner` left it passing. The audit found that
one; I found the first one by mutating.

Two guards, both green, both proving nothing, in a single task. Neither is visible from a passing
test run — that is the entire point. **A guard you have not watched fail is a guard you do not know
works**, and the only thing standing between that sentence and a false sense of coverage is the
mutation.

**Verdict: cheapest insurance in the protocol.** An hour against two guards that would otherwise have
been believed. Change nothing.

## Plan validation on every edit — the lowest, and it is nearly all clerical

**What it costs.** The validator runs after every plan edit, and under continuous cadence the plan is
edited constantly — after each step, each decision, each disposition. It is fast, but it is a
serialized gate on progress, and it rejected three times.

**What the three rejections were:**

| Rejection | Cause |
|---|---|
| `id cell "🔵 STEP-16 · in progress"` | `🔵` is not in the marker enum `⬜ 🟡 ✅ 🔴 ⬛` |
| `Status "complete" not in status enum` | The word is `completed` |
| Accepted gap `must end with "→ D-NN"` | The arrow was on the bullet's last wrapped line; the validator only reads the line starting with `- ` |

**None of the three concerned whether any of the work was correct.** All three are shape. The third
cost the most, because the error message describes a rule the document appeared to satisfy — the
bullet *did* end with `→ D-59` — and the real rule (arrow on the lead line) is only discoverable by
reading the validator source, which is what I ended up doing.

**Verdict: keep the validator, drop the cadence.** Structural checks on a plan are worth having;
running them as a blocking gate after every incremental edit converts a lint into an interruption.
Validate at step boundaries, not at every write. And make the arrow rule's message say *where* the
arrow must sit, since that is the part that is not guessable.

## What the audits caught that nothing else would have

**Audit-01 falsified the bound spec's central mechanism before a line was implemented.**
`StudyUnitSubstrate.Fingerprint` folded the extract hash into the substrate and `StudyFingerprints.Hash`
prepended it to every node — so extraction fingerprinted its own output and could never reach a fixed
point. Building on that premise would have produced a paid loop.

**Audit-02 found a regression I had introduced and had not noticed.** Moving publication out of
`ExtractMaterializer` lost a guarantee the materializer gave for free: it could only publish *after*
materializing. The Assessment trigger discarded the null return and published unconditionally, so an
unsupported document emitted a completion with no artifact behind it — silently.

The 27 findings across both cycles were overwhelmingly one kind: *a mechanism applied on the branch
that motivated it and nowhere else.* An independent reader finds that. Format validation never will.

## Where cost was misspent

**The spec was bound before its premises were verified.** Audit-01's most expensive finding was a
claim about `StudyFingerprints` that a twenty-minute code read would have falsified. Bound-spec
immutability then forced a superseding spec and a sixteen-step rebuild. The ordering is the defect:
*verify premises against the code, then bind.*

**The protocol generated one of its own defects.** Audit-02 FINDING-01: I hashed 50 files, launched
the audit, then kept the plan current while the auditor read, so the target moved mid-audit. Under
continuous cadence, "keep the plan fresh" and "freeze the manifest" are in direct conflict, and
nothing in the protocol names the conflict.

## What the protocol structurally cannot reach

Seven defects came only from executing the system, and neither audit nor the 2,253-test suite found
any of them:

- six from the first real AppHost start — missing EventGrid environment, a port collision, an
  emulator exiting 132, the wrong chat-client stack, a missing `prompts.yaml`, the wrong embedder;
- one from the first paid run — the fingerprint disagreement above, only observable across two
  processes and two runs, so no in-process test can see it.

**Running the thing is not a verification step among others; it is the only instrument for a whole
class of defect**, and it belongs earlier than "after structural green".

## What I would change

1. **Verify spec premises against the code before binding.** Cheapest possible change, prevents the
   most expensive failure observed here.
2. **Run the system early, on a thin path.** Six of seven runtime defects were configuration, not
   logic — reachable by one deliberately trivial end-to-end run long before feature completion.
3. **Validate the plan at step boundaries, not every edit**, and make the arrow rule state where the
   arrow goes.
4. **Freeze manifested files between audit launch and capture**, in the protocol rather than as a
   rediscovered finding.
5. **Keep mutation proof exactly as it is.**
6. **Add a register review at completion.** Reviewing `plan/deferred.md` against the finished work
   found five stale rows — including one whose revisit condition read "not before", about work the
   task had just unblocked. A row that tells the next reader to stop is worse than no row.

## Verdict

The ceremony earns its cost where it is adversarial — audits, mutation proof, register review — and
close to nothing where it is clerical. The distinction is not ceremony versus speed. It is whether
the mechanism can tell you something you did not already believe.
