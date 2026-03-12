# Razor Enhanced Fine-Tuning Dataset Builder

## Goal

Generate **10 new high-quality examples** to fine-tune an AI model that writes scripts using **Razor Enhanced APIs**.

The dataset must be stored in **`dataset.jsonl`**. The file MUST be strictly encoded in **UTF-8** to preserve special characters from different languages.

Each line must be a valid JSON object structured EXACTLY like this (using the Reasoning Distillation technique):

```json
{"messages": [
  {"role": "system", "content": "You are a Razor Enhanced (IronPython) expert. You MUST ALWAYS reason inside <think></think> tags before writing the code."},
  {"role": "user", "content": "Cura se vita sotto 70 con benda (0x0E21)"},
  {"role": "assistant", "content": "<think>User speaks Italian. APIs: Items.UseItemByID, Player.Hits, Target.Self, Target.WaitForTarget. IDs: 0x0E21.</think>\nimport Misc, Player, Items, Target\n\nif Player.Hits < 70:\n    Items.UseItemByID(0x0E21, -1)\n    Target.WaitForTarget(1000, False)\n    Target.Self()"}
]}
Each line must contain exactly one JSON object.

Mandatory Workflow (for every example)
Read the user prompt.

Read the Razor API Enhanced documentation "C:\Users\fabio.dagostino\Desktop\dataset\Merge e Censimento API Documenti.docx"

Write the script using only documented APIs.

THINK BEFORE CODING: The assistant's response MUST ALWAYS start with a <think>...</think> block written in English. It must briefly state the user's language, the required APIs, and the Hex IDs.

Provide the IronPython code maintaining exact 4-space indentation and \n line breaks.

Accuracy Rules
Never invent APIs

Never guess undocumented behavior

Provide exact Hex IDs if requested in the prompt.

If you are not fully certain of the correct implementation, skip that specific example and create a different one.

Multilingual Prompts
The "user" message must be randomly written in one of the following languages:

English

Italian

Chinese

Russian

German

Spanish

Language selection must be random for each example. This is critical for model training quality.

Dataset Generation Rules & Batch Size
Your objective is to generate exactly 10 NEW examples.
However, due to LLM output token limits, DO NOT generate them all in a single response.

Execution Strategy for Local Agent:

Check if dataset.jsonl exists. If it exists, read it to avoid duplicating concepts.

Generate the 100 examples in batches of 10-20 at a time.

After generating a batch, append it to dataset.jsonl ensuring UTF-8 encoding.

Automatically continue your execution loop (or ask me to proceed) until exactly 100 new examples have been successfully appended.

Script Quality Requirements
Assistant outputs must:

Use only Razor Enhanced APIs.

Include required imports (import Player, Items, Misc, Target, etc.).

Be syntactically correct Python (no Javascript, no C#).

Be realistic gameplay scripts (Combat, Crafting, Looting, Movement, Buffs).

Contain only the JSONL code (no markdown explanations outside the JSON structure).