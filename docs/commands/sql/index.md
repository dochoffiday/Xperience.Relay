# SQL

Raw SQL against the Xperience database, for what the typed commands can't do. Both commands need an Admin key and are guarded: see [Guardrails](../../concepts/guardrails.md). Prefer a typed command when there is one.

<!-- relay:begin commands -->
| Command | Access | What it does |
|---|---|---|
| [`query-sql`](query-sql.md) | Admin | Executes a read-only SQL query against the Xperience database. |
| [`execute-sql`](execute-sql.md) | Admin | Runs SQL against the Xperience database in two steps. |
<!-- relay:end commands -->
