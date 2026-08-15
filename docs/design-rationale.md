# Protodog — Design Rationale

## Why this document exists

This carries the settled design decisions behind Protodog into the distribution repository, so a
maintainer (human or model) changes the protocol with its reasoning in hand. Full provenance —
the vendor-neutral release candidate, the independent AUDIT-01 cycle, the reconciliation records,
and the build plans — lives in the frozen archive (`/Users/pablo/source/tmp/protocol`, commit
`9ec6704`).

## Settled decisions (do not silently re-litigate)

1. **Claude-specific implementation.** The semantic core (`protodog/core/`) is host-neutral prose;
   the mechanics layer is Claude-native — entry skills, hooks, validators. Where an invariant is
   mechanically enforced, prose states intent and enforcement defines the boundary.
2. **Codex participates only outside the protocol**, at two file boundaries: independent auditor
   via the self-contained hash-pinned dispatch (standing selection; challenge and remediation stay
   internal on the top Claude model), and ideation engine whose syntheses enter as `inputs/` with
   cited provenance — never directly into `specs/` or `plan/`.
3. **Mini-protocols and utilities stay in the snippet registry** (host-portable text — the same
   expansion serves any host, including Codex). Entry points are skills, invoked by name — no
   snippet prefix can collide with them, so the original skills-only reservation of `p-protocol-*`
   was retired (re-ruled 2026-08-13) and the prefix now marks the mini-protocol family itself
   (`p-protocol-ideate-*`: snippets opening a governed multi-turn working session), with
   prefix↔description coherence enforced by the registry validator.
4. **Audit loop**: audits are selected at planning time as planned units; launch is a per-audit
   HIL authorization gate (cost + data egress — deliberate: no standing grant for repository
   content leaving the boundary); the guarded launcher takes exactly one argument, re-verifies
   manifest hashes, runs read-only with pinned model, and writes only the next unused report,
   immutable once written.
5. **Git delegation**: the agent owns stage/commit/internal branches/merges inside its managed
   worktree, hook-enforced; fetch/sync, rebase onto `main`, squash, landing, cleanup remain
   engineer-owned via guarded scripts. One Task or whole Program lands as one commit.
6. **Markdown-in-repo artifacts are the sole state authority**; harness task lists are one-way
   mirrors; harness IDs never enter canonical artifacts. Templates + write-time validators own
   the shape; enum drift, ID reuse, unmapped acceptance, and empty sections are mechanically
   rejected. Status lives in the identifier cell beside a status marker rather than in its own
   column, so the scannable glyph and the authoritative enum cannot drift apart. The marker set is
   five meanings reused across every row kind — not one glyph per status — because a reader should
   learn it once, and it is restricted to single codepoints since a variation selector is invisible
   in an editor and lost on copy-paste. Strikethrough is reserved for settled gates, and
   `## Contents` is validated against the actual sections, which makes the index an enforced
   invariant instead of decoration that rots.
7. **Program profile mechanics are inherited from native subagent worktree isolation**; the
   coordinator never cedes plan-write ownership or the Program worktree (this resolves the
   coordination-writer seam AUDIT-01 found in the predecessor). Concurrent writers remain gated
   by plan-declared topology and exact authority.
8. **Enforcement over prose; hooks route, validators decide.** Hooks are thin adapters; validator
   logic is host-neutral .NET file-based apps with zero package dependencies — no NuGet restore
   in the enforcement path. Kernel parsing is deliberately hand-wired: lenient Markdown parsing is
   the anti-goal for a rigid dialect (fenced blocks are masked; revisit only if the artifact
   dialect grows toward general Markdown).
9. **Distribution**: this repo is the marketplace (`kennel`); the plugin follows the engineer
   across repositories; snippets install editor-level via symlink; Boxer ships project-scoped.
   Scope of installation mirrors scope of portability.
10. **C# follows the firm style guide** (`tsu/.github/instructions/csharp.instructions.md`): tabs,
    Allman, mandatory control-flow braces, restrictive comment policy.
11. **Ideation hardens before planning, with Claude-native pressure** (added 2026-08-13). The
    `ideation-audit` skill dispatches fresh-context adversarial subagents from inside an ideation
    mini protocol — a grounded auditor with repository access and a conceptual auditor deliberately
    without it — so design concepts and logic are settled while they are still cheap to change. It
    creates no Protocol state and never satisfies Protocol assurance; findings are session
    material, and only `inputs/` with cited provenance carries them toward planning. The
    host-portable fallback is `p-audit` in a fresh session.
12. **Plans die; registers and history carry what outlives them** (added 2026-08-13). `plan/`
    holds only plan-id directories and the deferred register (`plan/deferred.md`). A plan may not
    go terminal as the sole carrier of a live obligation — every deferred issue and accepted gap
    lifts to the register or records its closure via disposition arrows (enforced). Lifted rows
    are self-sufficient because retirement — engineer-triggered, land-then-sweep — deletes
    terminal plan directories; Git history on `main` is the archive, which one-commit landing
    guarantees. `plan/` is never an ADR store: durable rationale moves to specs or repository
    documentation.
13. **Plan maintenance is proof-then-judgment, engineer-confirmed** (added 2026-08-13). The
    `plan-sweep` skill deletes nothing that the deterministic checker (`scripts/sweep-check.cs`:
    every plan terminal, obligations lifted, content landed) has not proven and the engineer has
    not confirmed; citation liveness is the one judgment step in between. The `deferred-review`
    skill proposes register transitions with evidence and applies only what the engineer
    ratifies — a fired trigger proposes activation, never silent closure. The survey is a
    command, not a document: no index artifact returns.
14. **Diagram-suited work keeps a working canvas — one doctrine across stations** (added
    2026-08-15; provenance: `inputs/ideation-visual-support-handoff.md`). The closing diagram
    companion of an ideation session proved the visual form and failed only on timing; the fix is
    content diagrams maintained from the baseline turn. A session-state map was explicitly
    considered and shelved — the pain is visual support on the problem content, not on session
    placement; do not re-propose it. The canvas is a working view under the Foundation rule —
    mirrored to, never from; it illustrates, the owning artifact rules — with fixed structure
    (master at fixed altitude with concern lanes and the landing rule; one-question zooms;
    alternatives drawn in the affected region plus one ring before the prose comparison) and a
    fixed legend. The portable clause lives in the `p-protocol-ideate-*` prose, the mechanics in
    the `working-canvas` skill, the gate-presentation contract in the profile prose. Implementer
    rulings on the handoff's open points: losing sketches are deleted on settlement (the
    conclusions record keeps why they lost); ~ten boxes is a number loosely held; mixed detection
    cues default canvas-on (a discarded canvas is cheap); a diagram's existence never argues
    against reopening; continuity across stations is per-station seeding through the existing
    promotion rule, not one evolving file; an execution canvas retires with its plan unless
    explicitly graduated into documentation; built/verified renders purple (palette headroom).
15. **Authorization-gated assurance is interrogated at planning entry** (added 2026-08-15).
    Planning-time selection alone left the scheduling of audits and paid evaluation runs
    implicitly unsettled — daily use surfaced the pain as uncertainty about whether an instrument
    was considered, selected, or still pending. Task planning and each track's planning (Program-
    wide scheduling once, at Program planning) now open with the assurance interrogation, and
    exactly one outcome per instrument persists in the owning plan before readiness: selected
    with declared placement or run bounds, a decision gate when indicators support an instrument
    no selecting authority has ruled on, or an explicit not-scheduled decision with its basis.
    The negative is recorded deliberately — an absent answer is indistinguishable from an unasked
    question, which was the pain. The interrogation settles scheduling only: every launch keeps
    its per-instance authorization gate, and later evidence reopens the outcome as an ordinary
    material delta. Prose-level rule — no dialect, template, or validator change.
16. **Profile choice stays engineer-owned; a recommendation may be requested — and `p-protocol-*`
    is re-cut to carry it** (added 2026-08-15). The cold-start gap: work that skips ideation
    reached the Task/Program fork with no orientation, and the choice is semi-costly to reverse
    (supersession transfers nothing automatically). `p-protocol-recommend-profile` returns a
    decision-ready orientation — recommendation with confidence, driving criteria with evidence,
    flip conditions, and the suggested invocation line. Pull-only by design: never solicited by
    an entry skill, never a selection, no Protocol state — the Foundation states the rule, so the
    recommender cannot drift into scoring the choice. It is a one-shot prompt with no
    Claude-native mechanics, so it stays registry-side per decision 3; because it is not a
    multi-turn session, the family rule was re-cut a second time (engineer-ruled; see decision
    3's first re-ruling): `p-protocol-*` now marks the protocol-affiliated family, with the kind
    declared in the description — "mini protocol" (governed multi-turn session) or "protocol
    utility" (one-shot) — and the registry validator enforces prefix ↔ declared kind.
17. **Sessions retro themselves; remediations leave as handoffs** (added 2026-08-15). The
    working-canvas amendment (decision 14) was produced by an ad-hoc retrospective at the tail of
    an ideation session; `p-protocol-retro` codifies that practice as a mini protocol. It
    examines how the session went, never what it concluded: churn (items that iterated beyond
    their substance, and what form would have settled them sooner), uncertainties that dragged,
    recurring misreads revealed by engineer corrections, protocol-fit friction distinguished from
    session-local accident, and the practices that worked. Observations cite concrete session
    moments and carry provenance (engineer-stated versus inferred); corrected misdiagnoses are
    recorded so they are not re-proposed; remediations are shaped as handoffs with open points,
    not implementation plans. Prose-side complement to the hook-denial telemetry: denial spikes
    catch mechanical protocol-fit regression, the retro catches the judgment-side pain.

## Accepted background rationale

- The protocol encodes the engineer's pre-AI methodology: custom fit, a personal delegation
  vocabulary, per-session variance elimination, and drift detection across model updates justify
  it at any team size, including n=1.
- AUDIT-01 (of the predecessor) found pure-prose sections held the seams while tool-backed
  sections were sound — the reason weight sits in enforcement, not normative volume.
- Rent the universal (harness mechanics), own the differentiated (methodology encoding).
- Hook denial logs are protocol-fit telemetry: denial spikes after a model update signal
  regression; a hook that never fires is a deletion candidate.
