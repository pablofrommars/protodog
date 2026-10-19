# audit-01-dispatch — independent adversarial audit of the Protodog protocol

## Identity and output

- Audit: `audit-01` of the Protodog protocol as a whole, protocol-review cycle, 2026-10-19.
- Target repository: the repository root this dispatch lives in (`protodog` plugin marketplace
  repository). Working tree identity: commit `53e9101` on `main` plus the uncommitted release
  3.8.0 working-tree changes; the exact target state is the content-hash manifest below, which
  the launcher re-verifies before you start. Treat the manifest as the single source of truth for
  what is under audit.
- Output: your final message is the complete audit report in Markdown, following the report
  contract below, and nothing else. The launcher captures it to `audit-01.md` next to this
  file and makes it immutable.

## Independence

You are an external auditor with no authorship of the audited state. Nobody has briefed you
beyond this file. You receive no suspected weaknesses, no priority hints, and no per-claim
instructions: within the declared scope the audit is undirected — you choose where to dig.
You are adversarial toward claims and symmetric about the result: a claim that holds is reported
as holding only for the coverage you actually inspected, and no finding is invented to fill a
report shape.

## What Protodog is (orientation only, not a claim under audit)

Protodog is an execution protocol for agent-assisted engineering, installed as a Claude Code
plugin: skills are entry points (Task and Program profiles, maintenance skills), hooks and
validators are enforcement, and Markdown artifacts in the repository (`inputs/`, `specs/`,
`plan/`) are the only execution state. `protodog/core/` is the normative authority; everything
else either enforces it, enters it, or documents it. `README.md` is the operations guide and
`docs/design-rationale.md` records why decisions were made; neither overrides the core.

## Declared scope

### Scope: unnecessary complexity, ceremony, or duplication

**Dimension-scoped.** This cycle covers the single standard dimension **unnecessary complexity,
ceremony, or duplication** across the entire manifested set, together with cross-artifact
consistency wherever it is needed to evidence a finding. The question the engineer wants
answered is whether every rule, artifact, gate, section, validator check, and step the protocol
requires earns its cost against the protocol's own stated purpose (`README.md` "What this is",
`docs/design-rationale.md` accepted background rationale, and the Foundation's invariants). A
rule that costs ceremony and prevents nothing observable, two rules that say the same thing in
two places, an artifact nobody reads back, a check that cannot fire, a step with no consumer —
these are the shape of finding this cycle exists for. Within that dimension the audit is
undirected: no area is flagged as suspect and no area is exempt.

Out of scope: the engineer's other repositories; model vendors' behavior; anything not in the
manifest. You may consult the neutral references for context, but findings must be grounded in
the normative set.

## Manifest — normative sources

**Normative core (the Protocol text; carries authority)**

| sha256 | path |
|---|---|
| `04cb8ab6861a7851632ed32df8cc02f427e4a4f72fe7efecc3a5f0d275f044a1` | `protodog/core/foundation.md` |
| `005607b9818a4a88193f7f8cfa56327c4f8562af30b5430ea11be8e6f59d327c` | `protodog/core/git-policy.md` |
| `ecf379116738702a36b0ae34e60b4ffd3108350fc7f5ce3a459d3cafdaa9113f` | `protodog/core/program-protocol.md` |
| `77b8ac444eda1f2799fc2fa4c63736f2a53100190d23ab935ce2d5cc02923b5d` | `protodog/core/task-protocol.md` |
| `4488941846b623948732f21a087a24515fa904eca7f1b1357ea36764716228ec` | `protodog/core/verification-and-assurance.md` |

**Enforcement (hooks, validators, launcher, sweep checker, and their tests)**

| sha256 | path |
|---|---|
| `ad7390a74dba87a8fe787d773d14b163cbb8b429e99b4860db47a22fed0ec89d` | `protodog/hooks/hooks.json` |
| `d657526af2326594817409054924283a65f090c1ccb8c93f4d6dadc460a34a8f` | `protodog/hooks/protodog-hooks.cs` |
| `22fab7f92dfb25cd04ecc40ba8d985b3c4f8481d223218f2108979be99cdb9a3` | `protodog/hooks/settings-fragment.json` |
| `278622fc816d1fc7a49581f1e642b1c5eea496962b6032095954a4ec8511bb57` | `protodog/hooks/test-hooks.sh` |
| `c2ec49e17c7e594d0592a87afe7b392f4929ae9dda4f60c7fbf0188f5efae5d0` | `protodog/scripts/audit-launch.cs` |
| `e241fda1fdde38dd1c554d0ca078bb1644df90160b3f55a2dd2d8d8653b95975` | `protodog/scripts/sweep-check.cs` |
| `9faf912805e7e7df27e9a85e0c860f20977b1f80ef287fe63476290dd1677426` | `protodog/scripts/test-audit-launch.sh` |
| `0b2a00ab670bfedbf19b5ec2aeae395b270f140a2a4e3d14ab2fc28625ec9d5b` | `protodog/scripts/test-sweep-check.sh` |
| `d6f4a2d23e095e987de0c3eab2e90f2f4e01b978c87214ad206a8fde329d89ec` | `protodog/validators/test-registry.sh` |
| `0ce028073f33f80d3a426903d031de815a3f9d240686d63664aaa57cc934b05b` | `protodog/validators/test-validators.sh` |
| `8c05c96612a1c623176a0508c9fc9d2364fce32e01399219364b7653e1849b15` | `protodog/validators/validate-plan.cs` |
| `bf6460ef4b4d2910e4a8437979fca7ba5932980c7362803aecd3041c5ae62cc8` | `protodog/validators/validate-registry.cs` |

**Entry points and mechanics (skills, pinned agent, templates)**

| sha256 | path |
|---|---|
| `cb97244fa68eb4b38a8ed3f02aaef137871e5310b2018ee52f4e545cb9c4cb8d` | `protodog/.claude-plugin/plugin.json` |
| `a0eff62323be98e6f26ad05ce9679c791fbc9f861dda3c5870db0a7b3117b817` | `protodog/agents/challenge.md` |
| `c89af9f88b6eab7846c4800686ed241d799a0ca2b2d5a12dc5c55497f31f4c63` | `protodog/skills/deferred-review/SKILL.md` |
| `333f83959ccc5fb64d98c6eee09387be4845de69163db39079319c46b4a284c2` | `protodog/skills/ideation-audit/SKILL.md` |
| `195ad12f2dbe88705fc384a155ee19863b0f9d720036ff15ab1e56dd41f87fe5` | `protodog/skills/plan-sweep/SKILL.md` |
| `105045d89cc1384910d44f49fcae25c5376b7230c8e1c028d03edc88088da1e2` | `protodog/skills/program/SKILL.md` |
| `615dac13dfbc60bd053ed2a43b66a18a0e74698e991aad4c049c9d59b6b0b957` | `protodog/skills/task/SKILL.md` |
| `3f84f284e05808de380a6b5de1a3b02b03a3f14c79e4f47e902cf895fc1564a1` | `protodog/skills/working-canvas/SKILL.md` |
| `c23ca588b70f9ae79f3fb8bc83d8c090ccb5a8854f692a239a7040a06e39e1c3` | `protodog/templates/deferred-register.md` |
| `0c73d6204667c268ba746df239e763c2a74ff21bde0c8c87237ddd0068b21d51` | `protodog/templates/dispatch.md` |
| `00992b4b89a8b99565ef5c3317b805f87ecbe4890c47ee1ffae9645fb2f42e26` | `protodog/templates/program-plan.md` |
| `ca54e9d78408f7f9656b43fec99a2dd0b73eb313b96819009c7666d894771f48` | `protodog/templates/spec.md` |
| `ecf4204d291d0695441fc4dc895755fa0945cb570cfd50e2a40ce919e52c50f2` | `protodog/templates/task-plan.md` |
| `1241185807c3b5d57e797f6eb7694ea0420c7cbf86be4dca4498f6706c54def4` | `protodog/templates/track-plan.md` |

**Host-portable prompt registries**

| sha256 | path |
|---|---|
| `556558b3887f6ddefb1d782f816eb65d707e773530eb9d1f17068d0b4d058c52` | `protodog/registries/boxer.code-snippets` |
| `256b915b6dc78c9349c4e477bc54d828afd135260694a50ab4e7e8fc9333093b` | `protodog/registries/prompt-registry-style.md` |
| `4ec60f7de4a501cb9c813fb3b5bca39ab5d2c2d2d6c8eb4335316c9f2d9c150e` | `protodog/registries/prompt-utility.code-snippets` |

**Operations guide and design rationale (non-normative; README never overrides core)**

| sha256 | path |
|---|---|
| `d4dc44f52c2f8283b3af51041bba64674ad371ebce14e05d39e473a3bad5c1b3` | `README.md` |
| `4000eee88cf6ed10bcd6be3f79c95a1a205732c4844602150cc0bed0ee47d5f8` | `docs/design-rationale.md` |

## Neutral references (context only; separately labeled; not under audit)

| sha256 | path |
|---|---|
| `f2a46d6161c0f564e79dc2d0663d13fc526da340ebdf524abce3f2a9c69aa2c0` | `CHANGELOG.md` |
| `3b6b1dd472196bb8dabb19885264e91d2a515c003cf15e5acafb2cb1996ad9c1` | `inputs/protocol-cost.md` |
| `e59e77bf440a65932c1b0e47d02b58bbce09711e4659062643a5fd17c10a0cc9` | `inputs/ideation-visual-support-handoff.md` |

`CHANGELOG.md` records the release history and the stated reason for each change.
`inputs/protocol-cost.md` is the engineer's own cost retrospective after the first heavyweight
deployed Task; `inputs/ideation-visual-support-handoff.md` is an ideation handoff. These are
inputs, not authority: use them as evidence about intent and observed cost, never as a substitute
for inspecting the normative text.

## Evidence locations and inspection constraints

- Repository access is read-only. Read every manifested file; the normative core is roughly
  1,900 lines of Markdown and the enforcement roughly 2,000 lines of C# and shell.
- Test suites exist (`protodog/validators/test-validators.sh`, `test-registry.sh`,
  `protodog/hooks/test-hooks.sh`, `protodog/scripts/test-sweep-check.sh`,
  `test-audit-launch.sh`). Running them is optional and only if your sandbox permits; if you do
  not run them, say so under limitations. Static reading is sufficient for this audit.
- No network access is required or expected.
- There are no plan directories in this repository; `plan/` is absent. Do not treat that as a
  finding about the protocol.

## Standard dimensions

Considered where applicable, never a scorecard: outcome and acceptance correctness and
completeness; internal and cross-artifact consistency including state and vocabulary integrity;
evidence, verification, measurement, and evaluation integrity; compliance with scope,
constraints, invariants, authority, decisions, and applicable policy; failure, recovery,
concurrency, integration, security, data, operability, and maintainability risks; unnecessary
complexity, ceremony, or duplication.

## Report contract

Produce `audit-01.md` with, in order:

1. **Scope and target state** — restate the declared scope and the target-state identity
   (commit `53e9101` plus the manifest).
2. **Sources used** — which manifested files and neutral references you actually read.
3. **Coverage** — what was inspected, what was inaccessible, limitations, and material methods.
4. **Overall assessment** — concise, bounded by the stated coverage.
5. **Findings** — zero or more atomic findings, each as `FINDING-NN` with: claim; exact evidence
   (file and line or quoted text); affected surface; concrete consequence; confidence `high /
   medium / low` tied to the evidence. No mandatory severity scale. A recommendation per finding
   is optional and advisory; keep it to one sentence and keep it separate from the claim.
6. **Clean areas** — only what you inspected and found to hold, stated as coverage.

## Authority boundaries and prohibitions

- Read-only. Do not modify, create, or delete any file other than emitting your final message.
- Do not remediate, redesign, or plan. You audit claims; you do not propose a new protocol.
- Do not grade against an external methodology or a scorecard; grade the protocol against its
  own stated purpose, invariants, and rationale, and against ordinary engineering judgment.
- Recommendations never substitute for a finding: every recommendation must be attached to an
  evidenced finding.

## Coverage disclosure

A clean result applies only to the stated coverage. State explicitly any manifested file you did
not read and any dimension you did not consider, and why.
