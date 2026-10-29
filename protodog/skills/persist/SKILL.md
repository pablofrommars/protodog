---
name: persist
description: Persist the current session's conclusions and continuation context to an explicit destination, opening with "Why this document exists". Engineer-invoked with the destination path as argument.
disable-model-invocation: true
argument-hint: [@destination/path] [audience] [what to preserve]
---

# Persist session

You are persisting the current session to a document another session can continue from. This is
a utility, not a Protocol step: it creates no plan, spec, or register state. When the destination
is under `inputs/`, the document becomes ideation material with cited provenance, the only form
in which session results enter Protocol planning.

## Arguments

The invocation text supplies, in plain language and in any order: the destination path
(required; ask for it when absent, nothing else), the audience (default: engineers and LLMs),
and what to preserve (default: the conclusions plus only the context needed to understand and
apply them). On Claude Code the text replaces `$ARGUMENTS`; on Codex it follows the skill
mention.

<task>
Persist this session as instructed: $ARGUMENTS

Write for the stated audience, by default engineers and LLMs; include enough context for another session to continue.
Preserve what is stated, by default the conclusions plus only the context needed to understand and apply them; omit thought process and session narrative.
Open the artifact with `Why this document exists`. Cite provenance — the session's date and subject, documents read, decisions the engineer made versus recommendations — so the document stands without the conversation.
Write the file at the destination; do not create any other file, and do not overwrite an existing document without saying so first.
</task>
