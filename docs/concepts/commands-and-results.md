# Commands and results

Everything Relay does is a **command**: a verb, such as `create-web-page`, and a set of parameters. Every command answers with the same **result** shape.

## The envelope

A command travels as an envelope with two parts:

```json
{
  "verb": "get-page-info",
  "parameters": { "webPageId": 9, "languageName": "en" }
}
```

- `verb` names the command. The [command list](../commands/index.md) has them all, and `GET /verbs` lists the ones a deployed site supports.
- `parameters` holds the command's properties as a JSON object.

The envelope also has an `as` property. It is reserved: Relay accepts it and does nothing with it, so commands in a batch can't refer to each other's results.

### How parameters are read

- **Names are camelCase** on the wire (`webPageId`), and they are matched without regard to case, so `WebPageId` works too.
- **Unknown names are ignored.** A parameter that doesn't exist is dropped without an error, so check your spelling: a misspelled name leaves the real parameter at its default.
- **A missing `parameters` is the same as `{}`**, which suits commands that take nothing, such as [`get-taxonomies`](../commands/taxonomies/index.md#get-taxonomies). A `parameters` of `null` is a failure.
- **A value of the wrong type fails the command** and the message names the property: `The JSON value could not be converted to System.Int32. Path: $.webPageId`.
- **Optional parameters** can be left out. Where a parameter has a default (`languageName`, for one), the command's section says so; see also [Defaults and languages](defaults-and-languages.md).

## The result

```json
{
  "success": true,
  "message": "Moved web page 25 under parent 5.",
  "error": null,
  "data": null
}
```

| Property | Meaning |
|---|---|
| `success` | `true` when the command was carried out. |
| `message` | A sentence about what was done, for commands that report one. |
| `error` | Why the command couldn't be carried out. Only set when `success` is `false`. |
| `data` | The command's payload, for commands that return one. Each command's section says which type it is. |

The types are listed on the [Types](../reference/types.md) page. In C#, `result.GetData<T>()` reads `data` as the type you expect.

A result never claims more than happened. Where Kentico answers a request with a plain `false`, or quietly does nothing, Relay checks first and reports it: a delete of something that isn't there fails instead of succeeding, and a publish of something already published says so.

## HTTP status and the result

| Situation | HTTP status | Body |
|---|---|---|
| The command ran, whatever the outcome | `200` | A result. `success` tells you how it went. |
| The key is missing or unknown | `401` | None. |
| The key's tier is too low | `403` | A failed result naming both tiers. |
| The request body isn't valid JSON | `400` | ASP.NET Core's error. |
| The body is larger than the limit | `413` | ASP.NET Core's error. |

So check `success` on every `200`: a failed command is not an HTTP error. [Errors](../reference/errors.md) lists the messages you will meet.

## Reading `data`

`data` holds a different type for each command, which is why the result isn't generic: since it all travels as JSON, a type parameter would add nothing. The type is part of the command's section, and in C# the command's class tells you which type to ask `GetData<T>()` for.

Two things are worth knowing about values inside `data`:

- **Dates** are ISO 8601 strings, mostly without a time zone.
- **Reference, tag and asset fields** in `get-page` and `get-content` come as the JSON text Kentico stores: a string that contains JSON. The query commands embed the same values as real JSON instead.

## Batches and discovery

[`POST /batch`](../reference/http-api.md#post-batch) runs a list of commands in order and returns one result per command; see [Run several commands at once](../guides/batches.md). [`GET /verbs`](../reference/http-api.md#get-verbs) lists what the site supports.
