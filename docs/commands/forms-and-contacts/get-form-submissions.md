# get-form-submissions

<!-- relay:begin header -->
| Verb | Access | Returns | C# command |
|---|---|---|---|
| `get-form-submissions` | [Read](../../concepts/access-tiers.md) | [`GetFormSubmissionsResult`](../../reference/types.md#getformsubmissionsresult) | `GetFormSubmissionsCommand` |
<!-- relay:end header -->

Reads the submissions of a Kentico form, newest first.

<!-- relay:begin parameters -->
## Parameters

Names are shown the way they appear in the JSON; a C# command uses the same names in PascalCase.

| Parameter | Type | Required | Description |
|---|---|---|---|
| `formClassName` | string | Yes | Class name of the form, as shown in the admin under the form's properties, e.g. `BizForm.DancingGoatContactUs`. |
| `top` | integer | No | Maximum number of submissions to return. All of them when omitted. |
| `whereClause` | string | No | A SQL WHERE expression, without the `WHERE` keyword, over the form's table columns. Passed to Kentico as given. |
| `orderBy` | string | No | A SQL ORDER BY expression, without the keywords. Defaults to `FormInserted DESC`. |
<!-- relay:end parameters -->

<!-- relay:begin result -->
## Result

A successful run returns a [GetFormSubmissionsResult](../../reference/types.md#getformsubmissionsresult) in `data`:

| Field | Type | Description |
|---|---|---|
| `formClassName` | string | The class name of the form, as it was given. |
| `columns` | string[] | Column names, taken from the first submission, so they are empty when there are no submissions. |
| `rows` | string[][] | One list of values per submission, in `columns` order. Every value is text; null for SQL NULL. |
<!-- relay:end result -->

## Behavior

- **The form.** `formClassName` is the form's class name, shown in the admin under the form's properties; the examples use `BizForm.DancingGoatContactUs`. A name that isn't a form fails with "Form '...' was not found."
- **Columns and rows.** `columns` are the columns of the form's table (its fields, the submission's ID, `FormInserted` and `FormUpdated`), taken from the first row, so they are empty when there are no submissions. Every value is text, formatted the way the server formats it (a date reads like `10/19/2026 2:05:00 PM`), and `null` for a SQL NULL.
- **Order and size.** Submissions come newest first (`FormInserted DESC`) unless `orderBy` says otherwise. `top` limits how many come back; without it all submissions do.
- **`whereClause` and `orderBy`.** These are SQL fragments over the form's table columns, written without the `WHERE` and `ORDER BY` keywords, for example `UserEmail LIKE 'grace@%'`.

<!-- relay:begin examples -->
## Examples

The examples were captured from a live site running the Dancing Goat sample. Replace `https://your-site.example` and `YOUR_API_KEY` with your own site and key.

### Read the newest submissions of a form

Without `orderBy`, the newest submission comes first.

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-form-submissions",
      "parameters": {
        "formClassName": "BizForm.DancingGoatContactUs",
        "top": 10
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetFormSubmissionsCommand
    {
        FormClassName = "BizForm.DancingGoatContactUs",
        Top = 10,
    });
    var data = result.GetData<GetFormSubmissionsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Retrieved 2 submission(s) for 'BizForm.DancingGoatContactUs'.",
  "error": null,
  "data": {
    "formClassName": "BizForm.DancingGoatContactUs",
    "columns": [
      "Form_2023_09_12_17_45ID",
      "FormInserted",
      "FormUpdated",
      "UserFirstName",
      "UserLastName",
      "UserEmail",
      "UserMessage"
    ],
    "rows": [
      [
        "2",
        "10/19/2026 2:05:00 PM",
        "10/19/2026 2:05:00 PM",
        "Grace",
        "Hopper",
        "grace@example.com",
        "Can I book the cupping room for a team event?"
      ],
      [
        "1",
        "10/18/2026 9:30:00 AM",
        "10/18/2026 9:30:00 AM",
        "Ada",
        "Lovelace",
        "ada@example.com",
        "Do you ship cold brew kits?"
      ]
    ]
  }
}
```

### Filter and sort the submissions

=== "curl"

    ```bash
    curl -X POST "https://your-site.example/api/relay/commands" \
      -H "X-Relay-Api-Key: YOUR_API_KEY" \
      -H "Content-Type: application/json" \
      -d '{
      "verb": "get-form-submissions",
      "parameters": {
        "formClassName": "BizForm.DancingGoatContactUs",
        "whereClause": "UserEmail LIKE '\''grace@%'\''",
        "orderBy": "FormInserted ASC"
      }
    }'
    ```

=== "C#"

    ```csharp
    var result = await client.ExecuteAsync(new GetFormSubmissionsCommand
    {
        FormClassName = "BizForm.DancingGoatContactUs",
        WhereClause = "UserEmail LIKE 'grace@%'",
        OrderBy = "FormInserted ASC",
    });
    var data = result.GetData<GetFormSubmissionsResult>();
    ```

Response (HTTP 200):

```json
{
  "success": true,
  "message": "Retrieved 1 submission(s) for 'BizForm.DancingGoatContactUs'.",
  "error": null,
  "data": {
    "formClassName": "BizForm.DancingGoatContactUs",
    "columns": [
      "Form_2023_09_12_17_45ID",
      "FormInserted",
      "FormUpdated",
      "UserFirstName",
      "UserLastName",
      "UserEmail",
      "UserMessage"
    ],
    "rows": [
      [
        "2",
        "10/19/2026 2:05:00 PM",
        "10/19/2026 2:05:00 PM",
        "Grace",
        "Hopper",
        "grace@example.com",
        "Can I book the cupping room for a team event?"
      ]
    ]
  }
}
```
<!-- relay:end examples -->

<!-- relay:begin errors -->
## Errors

A command that can't be carried out comes back with HTTP 200 and `success` set to `false`; `error` says why. These are real answers:

???+ failure "A form that does not exist"

    Parameters:

    ```json
    {
      "formClassName": "BizForm.NoSuchForm"
    }
    ```

    Response:

    ```json
    {
      "success": false,
      "message": null,
      "error": "Form 'BizForm.NoSuchForm' was not found. Use the form's class name, shown in the admin under the form's properties (for example BizForm.ContactUs).",
      "data": null
    }
    ```
<!-- relay:end errors -->

## See also

- [`delete-contacts`](delete-contacts.md)
- [`query-sql`](../sql/query-sql.md) for anything else in the database
