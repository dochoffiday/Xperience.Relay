# Contributing

## Layout

```text
src/        the five packages
tests/      unit tests, one project per package
tools/      Xperience.Relay.Docs: keeps the generated parts of these docs in step with the code
docs/       this site (MkDocs); mkdocs.yml is at the repository root
hooks/      a small MkDocs hook for the site
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
4. **The docs.** Add the verb to `Areas.cs` in `tools/Xperience.Relay.Docs` so it gets a section on the page of its area, add examples (below), and run the generator, which adds the section. Replace the `TODO` lines it leaves with the command's description, its behavior and its related commands.
5. **Tests.** Put the logic that doesn't need Kentico in a small class and unit test it, as `PublishingFlow` and `FieldValueChecker` are.

## The documentation

The site is built with MkDocs Material. Most pages are hand-written Markdown in `docs/`. The command pages mix both. There is one page for each area of commands, `docs/commands/<area>/index.md`, with a section for every command in it. The parts between `<!-- relay:begin NAME -->` and `<!-- relay:end NAME -->` are generated (a name ends in the verb, as in `parameters:create-web-page`), and everything else is yours to edit.

Generated from the code and its XML docs:

- the list of commands at the top of each area page, and in each command's section the table at the top, its parameters and its result, as well as the [types](reference/types.md) page
- the command index, the tier lists and the commands of the navigation (the lines of `mkdocs.yml` between `relay:begin nav-commands` and `relay:end nav-commands`)
- the curl and C# tabs and the response of each example

Each command is in the menu, and its entry is a link to its section: `commands/web-pages/#create-web-page`. Moving between the commands of an area therefore scrolls the page instead of loading another one. The heading of a section is its verb and the generator checks that it is there, since MkDocs can't check the links of the menu. The headings inside a section carry the verb in their ids (`{ #create-web-page-parameters }`), because every command has a "Parameters" and the ids of a page must be unique.

The table of contents on the right of an area page lists only the commands. The page says so in its front matter (`toc_depth: 2`) and `hooks/toc_depth.py` applies it; without it every command would add six more entries.

Examples live in `docs/examples/<verb>/<name>.json`, each with a `title`, an optional `note`, a `kind` of `error` for a failure, the `request` and the `response` as the site returned them. They were captured against the Dancing Goat sample. The generator builds the curl and C# tabs from the request, and fails if the parameters no longer fit the command.

To regenerate after changing a command:

```bash
dotnet build
dotnet run --project tools/Xperience.Relay.Docs
```

CI runs the same tool with `--check` and fails when a generated block is stale, a command has no section or a `TODO` is left in a page, and then builds the site with `mkdocs build --strict`, which fails on any broken link or anchor.

To preview the site locally:

```bash
pip install -r docs/requirements.txt
mkdocs serve
```

## Releasing

Set the version in `Directory.Build.props` and push a tag named `vX.Y.Z`. The `publish.yml` workflow packs the five packages at the tag's version and pushes them to NuGet. The docs site deploys from `master` with `docs.yml`.
