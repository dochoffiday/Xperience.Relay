# How Relay works

Relay has two halves. One runs inside your Xperience web app and does the work. The other is whatever calls it: the `RelayClient` library, or any tool that can send an HTTP request.

```text
 caller                               your Xperience web app
 +----------------+    HTTPS + key    +----------------------------------------------+
 | RelayClient or | -----------------> | /api/relay/commands   /batch   /verbs        |
 | any HTTP tool  |                    |        |                                      |
 |                | <----------------- |   API key check -> dispatcher -> handler     |
 +----------------+   JSON result      |                                  |           |
                                       |                        Kentico's own APIs    |
                                       +----------------------------------------------+
```

## Why the work happens inside the app

An Xperience site deployed to the SaaS environment can't be reached by external .NET applications, so a separate tool can't use Kentico's management API or the database. Relay puts the Kentico-aware code in the web app itself and lets callers reach it over HTTP. Only the packages that need the Kentico SDK depend on it, so the caller's side stays free of it.

## The packages

| Package | Role |
|---|---|
| `Xperience.Relay.Contracts` | The wire contract: one class per command, the result types, the envelope, and the access tiers. Shared by both halves. |
| `Xperience.Relay.Core` | Finds commands by verb (`RelayVerbRegistry`) and runs them through an optional pipeline of behaviors to their handler (`RelayDispatcher`). No MediatR. |
| `Xperience.Relay.Kentico` | One handler per command, calling Kentico's managers and providers. Also the API key store, its admin application and the service account. |
| `Xperience.Relay.Hosting` | The three ASP.NET Core endpoints and the API key filter in front of them. |
| `Xperience.Relay.Client` | `RelayClient`: turns a command object into a request and the answer back into a result. |

## What happens to a request

1. **Authentication.** The endpoint filter reads the API key from the `X-Relay-Api-Key` header and looks it up. A missing or unknown key is rejected with `401` before anything else happens.
2. **Authorization.** Every command has a minimum [access tier](access-tiers.md). If the key's tier is lower, the request gets `403` and nothing runs. A [batch](../guides/batches.md) is checked as a whole, before its first command runs.
3. **Routing.** The `verb` in the envelope picks the command class. An unknown verb comes back as a failed result, not an HTTP error.
4. **Deserialization.** The `parameters` object becomes an instance of the command class. Names are matched without regard to case; a value of the wrong type fails the command with a message that names the property.
5. **Dispatch.** The dispatcher runs the command through any registered pipeline behaviors, then through its handler.
6. **The handler.** It checks what it can up front (the fields against the content type, whether the item exists, what state it is in) and then calls Kentico, usually as the [service account](../configuration.md#serviceaccountusername).
7. **The result.** The handler returns a `RelayCommandResult`. An exception that escapes a handler is caught and returned as a failed result too, with the exception's type and message in `error`, and logged on the site.

The endpoint answers `200` whenever it could run the command to the point of an answer, including when the command failed. See [Commands and results](commands-and-results.md).

## Commands and handlers

A command is a plain class that implements `IRelayCommand` and carries its verb and access tier:

```csharp
[RelayCommand("get-page-info", Access = RelayAccess.Read)]
public class GetPageInfoCommand : IRelayCommand
{
    public int WebPageId { get; set; }
    public string? LanguageName { get; set; }
}
```

A handler implements `IRelayCommandHandler<TCommand>` and is resolved from dependency injection. `AddRelayCore` scans the assemblies you give it for command classes, and `AddRelayKentico` registers the handlers for all the commands in this package. A command that doesn't say which tier it needs defaults to **Admin**, so a new command is never open to lower tiers by accident.

Cross-cutting concerns such as logging or validation can go in an `IRelayPipelineBehavior`, registered with `AddRelayPipelineBehavior<T>()`. Behaviors run in registration order around every command. None are built in; the seam is there for you.

## Who the changes are attributed to

Kentico records who made a change. Relay runs every command as one fixed Kentico user, [`ServiceAccountUserName`](../configuration.md#serviceaccountusername). Kentico's management API uses that user for attribution only, not for permission checks, so what a key may do is decided by its tier, never by the user.

## Next

- [Commands and results](commands-and-results.md)
- [Drafts and publishing](drafts-and-publishing.md)
- [Design notes](../design-notes.md), for the decisions behind the shape of the library
