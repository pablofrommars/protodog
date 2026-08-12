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
   expansion serves any host, including Codex). Entry points are skills; `p-protocol-*`-style
   reserved prefixes are skills-only, enforced by the registry validator.
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

## Accepted background rationale

- The protocol encodes the engineer's pre-AI methodology: custom fit, a personal delegation
  vocabulary, per-session variance elimination, and drift detection across model updates justify
  it at any team size, including n=1.
- AUDIT-01 (of the predecessor) found pure-prose sections held the seams while tool-backed
  sections were sound — the reason weight sits in enforcement, not normative volume.
- Rent the universal (harness mechanics), own the differentiated (methodology encoding).
- Hook denial logs are protocol-fit telemetry: denial spikes after a model update signal
  regression; a hook that never fires is a deletion candidate.
