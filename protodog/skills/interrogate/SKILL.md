---
name: interrogate
description: Put the open choices of the current session, or of a stated topic, to the engineer through the host's structured question tool, recommendation marked and default stated. Engineer-invoked shortcut.
disable-model-invocation: true
argument-hint: [optional topic to narrow the choices]
---

# Interrogate

Turn the open choices into structured questions the engineer can answer by selection. This is the
decision interrogation of the Foundation applied on request, outside any gate: nothing here
creates authority, closes a gate, or writes Protocol state. Where a Task is live, a ruling given
here is persisted by the Task under its Decisions or Authority section as usual.

Topic from the invocation, if any: $ARGUMENTS

## Procedure

1. Collect the choices. Without a topic, every choice the session has left open or the engineer
   has deferred; with a topic, only those under it. Resolve first whatever available evidence can
   resolve, and drop those — a question evidence already answers is not asked.
2. Open with the reassessment, two or three lines: what the answers would change and what holds
   regardless. The engineer reads this before the questions, never after.
3. Ask through the host's structured question tool, within its batch limit per round, more rounds
   when needed. Each question stands alone, names the items it is about, and carries two to four
   options; the recommended option comes first and is marked as recommended, and the default that
   holds if the question is left unanswered is stated. Multi-select only when choices are not
   mutually exclusive. Without a structured question tool, ask the same batch as text in the
   decision-ready form: exact need, why it is the engineer's, recommendation, consequences, and
   the shortest unambiguous response shape.
4. After the answers, restate the rulings in one list — ruled, defaulted, left open — and continue
   the session on them. A question that came back unanswered is asked once more, alone; left
   unanswered again, it is recorded as open with its default, not asked a third time.
