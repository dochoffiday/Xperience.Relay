# Guardrails

Some commands are powerful enough that a mistake, or a misplaced key, would hurt. Relay puts checks around them. This page says what the checks are, and what they are not.

!!! warning "Guardrails are not a security boundary"
    The checks stop mistakes and the obvious abuses. They don't make an Admin key safe to hand out. An **Admin key is fully trusted**: it can do anything the site's database login can. Protect keys accordingly (see [Security](../security.md)).

## What's protected

The `Relay_ApiKey` table and the `relay.apikey` object type hold the API key hashes. They can't be reached through the generic commands, so a key can't read other keys' hashes or give itself a higher tier:

- [`query-sql`](../commands/sql/index.md#query-sql) and [`execute-sql`](../commands/sql/index.md#execute-sql) refuse any statement that mentions the table by name (in any letter case, schema-qualified or in brackets), and **also when the name is inside a string literal**, so it can't be tucked into dynamic SQL.
- The four custom-object commands refuse the object type with "Object type 'relay.apikey' is protected."

## How SQL is checked

SQL is not matched against keywords. Relay parses it with Microsoft's T-SQL parser (ScriptDom) and looks at the statements it finds, so a comment or a string literal can't hide a forbidden statement.

| | `query-sql` | `execute-sql` |
|---|---|---|
| `SELECT`, `WITH ... SELECT`, `DECLARE` | allowed | allowed |
| `SELECT ... INTO` a table | refused (a temporary `#table` is allowed) | allowed |
| `INSERT`, `UPDATE`, `DELETE`, `TRUNCATE`, `EXECUTE` | refused | allowed |
| `CREATE`, `ALTER`, `DROP` | refused | allowed |
| Anything else | refused | refused |

In both, these names are refused wherever they appear, in any statement: `xp_cmdshell`, `OPENROWSET`, `OPENDATASOURCE`, `OPENQUERY`, `OPENXML` and `sp_configure`.

Other limits:

- A query that doesn't parse is refused with the parser's message.
- Queries are cut off after [`SqlQueryTimeoutSeconds`](../configuration.md#sqlquerytimeoutseconds) (30 seconds by default).
- `execute-sql` works in two steps. Without `confirm: true` it only describes what would run; see [`execute-sql`](../commands/sql/index.md#execute-sql).
- Relay doesn't wrap statements in a transaction.

## Checks on content writes

Creating and updating pages and items checks the fields against the content type **before** anything changes, because Kentico would otherwise drop an unknown field without saying so, store linked items or tags as JSON text in whatever field it is told to, and fail on a value it can't store halfway through the write.

- Every field must exist on the content type (including fields from reusable field schemas). Names aren't case sensitive.
- Plain values must fit: a whole number for an integer field, a date that parses for a date field, `true` or `false` for a boolean, text no longer than the field allows, a decimal with no more digits than its column holds, and not `null` for a field that must have a value.
- `linkedItemFields` only go into content item reference fields, `tagFields` into tag fields and `assets` into asset fields.
- Every problem is reported in one answer, and nothing is created or changed when any check fails.

Values for reference, asset and tag fields sent through plain `fields` are not checked.

## Checks on other commands

Some commands check what Kentico would not:

- Deleting or moving something that doesn't exist fails instead of reporting success.
- [`move-tag`](../commands/taxonomies/index.md#move-tag) refuses a parent from another taxonomy and a move under the tag itself or one of its children.
- [`move-web-page`](../commands/web-pages/index.md#move-web-page) and [`move-content-item`](../commands/content-items/index.md#move-content-item) check the target and the items first.
- [`delete-contacts`](../commands/forms-and-contacts/index.md#delete-contacts) refuses an empty condition.

## What guardrails don't do

- They don't limit **what an Admin key can read or change** through SQL beyond the rules above.
- They don't replace **HTTPS**, key hygiene, or network controls in front of the site.
- They don't make a Kentico operation reversible: only the recycle bin, which pages and items use by default, offers a way back.
