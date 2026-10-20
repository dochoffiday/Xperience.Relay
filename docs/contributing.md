# Contributing

## Layout

```text
src/        the five packages
tests/      unit tests, one project per package
tools/      Xperience.Relay.Docs: keeps the generated parts of these docs in step with the code
docs/       this site (MkDocs); mkdocs.yml is at the repository root
```

## Build and test

```bash
dotnet build
dotnet test
```

The unit tests cover everything that doesn't need a Kentico database: the draft and publishing rules, field checking, the SQL checks, the dispatcher, the API key authorization and the wire format. The Kentico handlers themselves are verified against a real Xperience database; when you change one, run it against a site that has the Dancing Goat sample, including its failure paths.

Packages ship their XML documentation, and the build treats a broken `<see cref>` as a warning, so keep it at zero.

## Adding a command

1. **The contract.** Add a class to `src/Xperience.Relay.Contracts/Commands`, tagged `[RelayCommand("your-verb", Access = RelayAccess.X)]`. Give the class and **every property** an XML summary: the docs are generated from them. Put the sentence "Required." in the summary of a required property, and end the class summary with "Returns a `<see cref="..."/>` in `<see cref="RelayCommandResult.Data"/>`." if it returns data. Add the result type, with every property documented, next to it.
2. **The handler.** Add a class to `src/Xperience.Relay.Kentico/Handlers` implementing `IRelayCommandHandler<TCommand>`, and register it in `AddRelayKentico`. Check what you can before calling Kentico, and make a failure say so: never report a success that didn't happen.
3. **The tier.** Pick the lowest tier that is safe. Leaving `Access` out makes the command Admin-only.
4. **The docs.** Add the verb to `Areas.cs` in `tools/Xperience.Relay.Docs` so it gets a page, add examples (below), and run the generator. See below.
5. **Tests.** Put the logic that doesn't need Kentico in a small class and unit test it, as `PublishingFlow` and `FieldValueChecker` are.

## The documentation

The site is built with MkDocs Material. Most pages are hand-written Markdown in `docs/`. The command pages mix both: the parts between `<!-- relay:begin NAME -->` and `<!-- relay:end NAME -->` are generated, and everything else is yours to edit.

Generated from the code and its XML docs:

- the table at the top of each command page, its parameters, its result and the [types](reference/types.md) page
- the command index, the tier lists and the navigation
- the curl and C# tabs and the response of each example

Examples live in `docs/examples/<verb>/<name>.json`, each with a `title`, an optional `note`, a `kind` of `error` for a failure, the `request` and the `response` as the site returned them. They were captured against the Dancing Goat sample. The generator builds the curl and C# tabs from the request, and fails if the parameters no longer fit the command.

To regenerate after changing a command:

```bash
dotnet build
dotnet run --project tools/Xperience.Relay.Docs
```

CI runs the same tool with `--check` and fails when a generated block is stale or a command has no page, and then builds the site with `mkdocs build --strict`, which fails on any broken link.

To preview the site locally:

```bash
pip install -r docs/requirements.txt
mkdocs serve
```

## Releasing

Set the version in `Directory.Build.props` and push a tag named `vX.Y.Z`. The `publish.yml` workflow packs the five packages at the tag's version and pushes them to NuGet. The docs site deploys from `master` with `docs.yml`.
