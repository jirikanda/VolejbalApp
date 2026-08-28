# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

VolejbalApp is a Czech-language volleyball signup app built on the HAVIT .NET framework stack: a **Blazor Web App with WebAssembly interactivity** — `Web.Client` (WASM UI) is hosted by `Web` (ASP.NET Core host that also exposes the REST API the client calls). Solution file is `VolejbalApp.slnx`, targeting **.NET 10** (`net10.0`). Identifiers and domain language are Czech (Termín = scheduled session, Přihláška = signup, Osoba = person, Vzkaz = bulletin-board message).

## Common commands

All commands run from repo root unless noted.

```powershell
# Restore / build / test (mirrors .github/workflows/build.yml)
dotnet restore VolejbalApp.slnx
dotnet build VolejbalApp.slnx --configuration Release
dotnet test Tests/Tests.csproj --configuration Release

# Run a single test (uses Microsoft Testing Platform — see global.json)
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~VolejbalDbContext_CheckModelConventions"

# Publish Web (the WASM client is bundled into the host output automatically).
# Note: deploy.yml does not publish plain artifacts — it publishes a container image instead.
dotnet publish Web/Web.csproj --configuration Release --output ./publish/Web

# Apply EF migrations + run data seeds (deployment-time; also needed before first local run)
dotnet run --project MigrationTool -- --connectionstring "Data Source=(localdb)\mssqllocaldb;Initial Catalog=VolejbalApp;Application Name=VolejbalApp-MigrationTool;Trust Server Certificate=True"

# Regenerate Repositories / DataSources / Metadata from Model/* entities
# (writes into Model/_generated and DataLayer/_generated)
./DataLayer/Run-CodeGenerator.ps1
# or:  dotnet tool restore && dotnet efcodegenerator   (run with DataLayer as cwd)

# Regenerate typed API clients after changing controllers / DTO shapes
# (builds Web to refresh the OpenAPI document, then NSwag writes Web.Client/_generated/WebApiClients.cs — commit the result)
./Web.Client/Run-WebApiClientsGenerator.ps1   # run with Web.Client as cwd

# EF Core migrations — point tooling at Entity project, startup is Entity
dotnet ef migrations add <Name> --project Entity --startup-project Entity
dotnet ef database update --project Entity --startup-project Entity
```

The test runner is **Microsoft Testing Platform** (`EnableMSTestRunner=true` in `Directory.Build.props`, plus `global.json`), so test projects are `OutputType=Exe` and can also be launched directly: `Tests/bin/Release/net10.0/KandaEu.Volejbal.Tests.exe`.

`Release` builds set `TreatWarningsAsErrors=true` — warnings that build locally in Debug may break CI.

## Architecture

### Layering (referenced top → down)

```
Web.Client (Blazor WASM, runs in browser)  ──HTTP──►  Web (host: serves the client + REST API + background jobs)
                                                        │
                                                        ▼
                                                Facades  ── Contracts (DTOs + facade interfaces)
                                                        │
                                                        ▼
                                                Services (domain services, jobs, mailing)
                                                        │
                                                        ▼
                                                DataLayer  (repositories, DataSources, seeds)
                                                        │
                                                        ▼
                                                Entity  (DbContext, EF Configurations, Migrations)
                                                        │
                                                        ▼
                                                Model   (plain POCO entities)
```

`Web.Client` does **not** reference Facades or below — it only references `Contracts`. It calls the API over HTTP via NSwag-generated typed clients committed at [Web.Client/_generated/WebApiClients.cs](Web.Client/_generated/WebApiClients.cs) (namespace `KandaEu.Volejbal.Web.Client.WebApiClients`, System.Text.Json, `GenerateDtoTypes:false` — DTOs come from `Contracts`). Clients are wired in [Web.Client/Program.cs](Web.Client/Program.cs) via `AddCustomizedHttpClient` with base address = the host origin (`builder.HostEnvironment.BaseAddress`), so there is no API-URL config and no CORS.

**Regenerating the API clients**: the generated file is committed (same convention as `DataLayer/_generated`). After changing controllers or DTO shapes, run `Web.Client/Run-WebApiClientsGenerator.ps1` (builds `Web`, which emits the OpenAPI document via `OpenApiGenerateDocumentsOnBuild`, then runs NSwag CLI from the tool manifest) and commit the result. This script exists because a build-time `OpenApiProjectReference` would create a circular reference (`Web.Client → Web → Web.Client`). Client-side `JsonSerializerOptions` are aligned to the server's camelCase in [Web.Client/WebApiClients/WebApiClientsJsonSettings.cs](Web.Client/WebApiClients/WebApiClientsJsonSettings.cs) — keep that file in sync if new clients appear.

### HAVIT framework conventions (this is the biggest "you can't tell from the code" thing)

The repo is built on HAVIT's EF Core stack and conventions. Read these before changing data-access code:

- **Entities live in `Model/`** as plain POCOs. New entities go here.
- **`DataLayer/_generated/` and `Model/_generated/` are generated**. Do not hand-edit. After adding/changing an entity or `EntityConfiguration`, re-run `Run-CodeGenerator.ps1`. The generator produces: `IXxxRepository`/`XxxDbRepository` (+ Base + QueryProvider), `IXxxDataSource`/`XxxDbDataSource`, fakes, and `Model/_generated/Metadata/*`.
- **Hand-written repository extensions** sit in `DataLayer/Repositories/` (e.g. `OsobaDbRepository.cs`) — the generator emits `partial` bases so you can extend without touching generated code.
- **DataSources** (`IXxxDataSource.Data`, `.DataIncludingDeleted`) are the read-only `IQueryable` projection surface used by Facades. Repositories are write/lookup; DataSources are query.
- **`IUnitOfWork`** is the commit boundary — `AddForInsert`/`AddForUpdate`/`AddForDelete` then `CommitAsync`. See [Facades/Nastenka/NastenkaFacade.cs](Facades/Nastenka/NastenkaFacade.cs) for a canonical example.
- **DI registration is attribute-driven.** Mark a class `[Service]` (from `Havit.Extensions.DependencyInjection.Abstractions`) and it is picked up by `AddByServiceAttribute` in [DependencyInjection/ServiceCollectionExtensions.cs](DependencyInjection/ServiceCollectionExtensions.cs). Scoped lifetime is the default. Services targeted only at WebAPI use the `WebAPI` profile (see [Services/Infrastructure/ServiceProfiles.cs](Services/Infrastructure/ServiceProfiles.cs)).
- **Soft delete is a HAVIT convention.** Entities expose `Deleted` (DateTime?) and the generated `IXxxDataSource.Data` filters it out; use `DataIncludingDeleted` when you need both.

### Web (host) composition

- Single composition root is [DependencyInjection/ServiceCollectionExtensions.cs](DependencyInjection/ServiceCollectionExtensions.cs) via `ConfigureForWebAPI` (production) or `ConfigureForTests` (tests) — the method name and the `WebAPI` service profile string are kept from the pre-merge era, do not rename them casually (the profile is referenced by `[Service(Profile = ...)]` attributes). Both call `ConfigureForAll` which installs EF Core (SQL Server, or in-memory for tests), HAVIT services, and runs `AddByServiceAttribute` across `DataLayer`, `Services`, `Facades` assemblies.
- [Web/Startup.cs](Web/Startup.cs) wires MVC controllers, Razor Components (`AddInteractiveWebAssemblyComponents`), OpenAPI (built-in ASP.NET Core), rate limiting (`DefaultAPI` policy = 10/5s window with 10 queued — applied **only** to `MapControllers()`, not to Blazor/static assets), Application Insights, and exception monitoring (HAVIT).
- API reference UI is **Scalar** at `/scalar` ([Web/Infrastructure/ConfigurationExtensions/OpenApiConfig.cs](Web/Infrastructure/ConfigurationExtensions/OpenApiConfig.cs)); it and the `/openapi/current.json` document endpoint are mapped **only in Development**. The `AddOpenApi` service registration is gated on Development **or** running under `GetDocument.Insider` (detected via entry assembly in `Startup.ConfigureServices`) — that keeps the build-time document export for the client generator (`OpenApiGenerateDocumentsOnBuild`) working while production has no OpenAPI services at all. NSwag exists **only as a dev CLI tool** (`nswag` in the tool manifest) — no NSwag runtime packages. The `Controller_Action` operationId transformer and `OpenApiVersion = 3.0` in `OpenApiConfig.cs` are what keep the NSwag-generated client names stable — don't remove them.
- Blazor endpoints: `MapRazorComponents<App>().AddInteractiveWebAssemblyRenderMode().AddAdditionalAssemblies(...)` with the root document at [Web/Components/App.razor](Web/Components/App.razor). **Prerendering is off** (`new InteractiveWebAssemblyRenderMode(prerender: false)`) — the host renders only the static shell, all UI runs client-side; the host therefore does not register any client services.
- In Development, `DelayRequestMiddleware` (artificial 500 ms) applies **only under `/api`** so it doesn't slow WASM asset downloads; `UseWebAssemblyDebugging()` is enabled.
- **Background jobs run via [Services/Jobs/RecurringJobsBackgroundService.cs](Services/Jobs/RecurringJobsBackgroundService.cs)** (plain `BackgroundService` + `PeriodicTimer`; no Hangfire, no persistence, no dashboard). At startup it runs `IEnsureTerminyJob` + `IDeaktivaceOsobJob` (non-blocking, catch-up semantics for scale-to-zero), then `EnsureTerminy` hourly and `DeaktivaceOsob` once a day after 04:00 CET. A failed run is logged and retried at the next tick. Registration is gated on a non-empty `ConnectionStrings:Database` (so OpenAPI tooling can run without a DB). The scheduler is in-process — the app must run with **max 1 replica**.
- **The app does NOT migrate or seed the database at runtime.** Schema migrations + data seeds run via the **`MigrationTool`** console project at deployment time (see below) — the app assumes the schema already exists.
- **Cold-start warmup**: [Web/Infrastructure/WarmupBackgroundService.cs](Web/Infrastructure/WarmupBackgroundService.cs) is a `BackgroundService` that, once Kestrel is listening (`IHostApplicationLifetime.ApplicationStarted`), fires loopback HTTP `GET`s at `/api/nastenka`, `/api/osoby/aktivni`, `/api/terminy` — the endpoints the home page hits first. This has to be a real HTTP self-call, not a direct facade call from within the process: EF Core caches compiled queries per LINQ expression shape (warming a differently-shaped query, e.g. the one in `EnsureTerminyJob`, doesn't help), and calling the facade directly also skips MVC routing/controller activation/JSON serialization entirely — measured, a direct facade warmup left the first real request at the same ~100 ms as no warmup at all, while the HTTP self-call brings it down to ~5–40 ms (same as a warm request). Registration is gated on a non-empty `ConnectionStrings:Database`, same as the other DB-touching hosted services.

### MigrationTool (deployment-time migrations + seeds)

- Console app ([MigrationTool/Program.cs](MigrationTool/Program.cs)): applies EF Core migrations and runs `CoreProfile` data seeds, then exits. **CI/CD does not touch it** — it is neither published nor run by any workflow; it is a manual step from a local clone, both for production and for local dev.
- Wiring: `ConfigureForMigrationTool` in [DependencyInjection/ServiceCollectionExtensions.cs](DependencyInjection/ServiceCollectionExtensions.cs) (deliberately minimal — EF Core + DataLayer + [Services/Infrastructure/MigrationTool/MigrationService.cs](Services/Infrastructure/MigrationTool/MigrationService.cs); no Services/Facades).
- Parameters: `--connectionstring <cs>` (maps to `ConnectionStrings:Database`), optional `--commandtimeout <seconds>` (default 300); environment variables work too (`ConnectionStrings__Database`).
- **Local dev**: after cloning or adding a migration, run the MigrationTool (F5 profile uses LocalDB) — the Web app no longer creates/updates the DB for you. `TestsForLocalDebugging` is unaffected (it migrates+seeds itself per test).

### Web.Client (Blazor WebAssembly)

- Global WebAssembly interactivity without prerender; root components (`Routes`, `HeadOutlet`) are activated from the host's `App.razor` markers — [Web.Client/Program.cs](Web.Client/Program.cs) registers no root components.
- Czech locale is set in `Program.cs` (`CultureInfo.DefaultThreadCurrentCulture`); the csproj sets `BlazorWebAssemblyLoadAllGlobalizationData=true` because Czech is not in the default ICU shards — don't remove it or date formatting breaks.
- UI components: **Havit.Blazor.Components.Web.Bootstrap** (Hx* components). Use these instead of writing raw Bootstrap markup where one fits. Local state via `Blazored.LocalStorage`.
- Static assets (css/js/favicons) live in `Web.Client/wwwroot` and are served through the host's `MapStaticAssets()`.

### Tests

Two test projects, deliberately separated:

- **`Tests/`** — CI tests (run in `dotnet test` step of `build.yml`). Currently a minimal model-convention check. Uses MSTest + EF Core InMemory.
- **`TestsForLocalDebugging/`** — local-only debugging tests that hit LocalDB (`(localdb)\mssqllocaldb`). Base class [TestsForLocalDebugging/TestBase.cs](TestsForLocalDebugging/TestBase.cs) wires up the real DI container via `ConfigureForTests`, with `EnsureDeleted` + `Migrate` + seed between tests. Do not add these to CI.

## Configuration

- Web (host): `appsettings.Web.json` + environment override + (Debug only) `*.local.json` (gitignored) + env vars. Secrets (connection strings, AI) are supplied via environment variables in production — no Key Vault.
- Web.Client has no configuration files — the API base address is the host origin.
- The Entity project has its own `appsettings.json` (used by `VolejbalDesignTimeDbContextFactory` for EF tooling and the code generator).

## Build / coding conventions ([Directory.Build.props](Directory.Build.props) + [.editorconfig](.editorconfig))

- `Nullable` is **disabled** project-wide. Don't sprinkle `?`/`!` expecting NRT semantics.
- `ImplicitUsings` enabled; common HAVIT/EF usings are pulled in via per-project `GlobalUsings.cs`.
- `DisableTransitiveProjectReferences=true` — if you need a type from a non-direct dependency, add the explicit `ProjectReference`.
- Central package versions in [Directory.Packages.props](Directory.Packages.props) (`ManagePackageVersionsCentrally=true`). Add new packages by `<PackageVersion>` here, then `<PackageReference>` (no version) in the csproj.
- `.editorconfig` enforces tabs, file-scoped namespaces, usings outside namespace (`System.*` first), **explicit types over `var` (never use `var`)**, required braces, and parentheses-for-clarity in binary/relational expressions. Style violations are warnings; `Release` turns them into errors.
- Project-specific naming (full detail in the `CodeConventions` skill, [.claude/skills/CodeConventions/SKILL.md](.claude/skills/CodeConventions/SKILL.md)): instance fields `_camelCase`, static fields `s_camelCase`, **primary-constructor parameters also `_camelCase`** (e.g. `MailingService(IOptions<MailingOptions> _mailingOptions)`), fields are always `private`, async methods end with `Async` and take `CancellationToken` as the last parameter.

## CI / deployment

- [.github/workflows/build.yml](.github/workflows/build.yml) — CI for `master` and PRs into `master`: restore → build (Release) → test. Nothing is published from here; it only verifies the solution compiles and the tests pass.
- [.github/workflows/deploy.yml](.github/workflows/deploy.yml) — **manual only** (`workflow_dispatch`, run against `master`); the whole path to production in two jobs. `build`: restore → build → test → containerize `Web` via the .NET SDK's built-in containerization (`dotnet publish -t:PublishContainer`, no Dockerfile) and push `ghcr.io/<owner>/volejbal-web` tagged with the **commit SHA only** (deliberately no `latest` — a floating tag would be an implicit, unreviewed deploy target), authenticating with the workflow's own `GITHUB_TOKEN` (`packages: write`). `deploy` (`needs: build`): OIDC login + `az deployment group create` against [deploy/main.bicep](deploy/main.bicep). There is **no `imageTag` input** — the workflow builds and deploys the checked-out commit; triggering it manually *is* the decision of what goes to production. The two-job split is deliberate: `environment: Production` sits on `deploy`, so required reviewers approve after build and tests have passed, not blind at the start. The WASM client is bundled into the host image's `wwwroot/_framework`; `Web` is not published as a plain artifact — the container image is the deliverable.
- The deploy job applies the **whole template**, not just an image swap — ARM's incremental mode no-ops the unchanged resources, and in exchange the infrastructure cannot drift from `main.bicep`. Nothing has to tell the Container App about the new image: changing `containerImage` changes the app's `template`, which creates a **new revision**, and `activeRevisionsMode: 'Single'` shifts all traffic to it. (Secrets are an app-level property, not per-revision — changing only a secret value creates no revision and does not restart anything.)
- **Azure authentication is OIDC / federated credentials**, no stored password: `permissions: id-token: write` plus `client-id`/`tenant-id`/`subscription-id` on `azure/login`, which come from repository **variables** (`vars.AZURE_*`, not `secrets.*` — they are identifiers, not secrets; using the wrong context yields an empty string and an opaque login failure). The federated credential's subject must be `repo:jirikanda@5111719/VolejbalApp@169379851:environment:Production` — two separate traps in one string: it is an **environment** subject (the job targets `environment: Production`, so a ref-based subject silently fails to match), and the repo has **immutable subject claims** enabled, so the prefix carries owner and repo IDs (`@5111719`, `@169379851`) rather than just names. Check the live prefix with `gh api repos/jirikanda/VolejbalApp/actions/oidc/customization/sub`. `DATABASE_CONNECTION_STRING` is the only real secret left (the GHCR package is public, so the template has no `registries` block).
- [deploy/main.bicep](deploy/main.bicep) (see [deploy/README.md](deploy/README.md) for secrets and one-time setup) creates the Log Analytics workspace (`jk-volejbal-logs`), Application Insights (`jk-volejbal-appinsights`, workspace-based on that same workspace — its connection string is passed to the app by reference, so there is no App Insights GitHub secret), the Container Apps Environment (`jk-volejbal-ca-env`) and the Container App (`jk-volejbal-ca-web`) — all names follow a `jk-volejbal-<role>` lowercase-kebab convention, forced by the Container Apps naming rules; **the database and the custom domain are out of its scope**. Schema migrations are **not** touched by the pipeline at all — it neither publishes nor runs `MigrationTool`. Run it by hand from a local clone (`dotnet run --project MigrationTool -- --connectionstring "…"`) against production *before* triggering a deploy that needs a new migration.
- ReadyToRun gotchas (learned the hard way): `PublishReadyToRun` is set in [Web/Web.csproj](Web/Web.csproj) conditioned on `RuntimeIdentifier != ''` — passing `-p:PublishReadyToRun=true` on the CLI would flow into `Web.Client` (browser-wasm) and fail with NETSDK1095. Similarly, never `dotnet restore` the whole solution with `--runtime` — Web.Client would demand a non-existent Mono runtime pack; the container publish step in `deploy.yml` does its own RID-specific restore instead. [MigrationTool/MigrationTool.csproj](MigrationTool/MigrationTool.csproj) carries the same `PublishReadyToRun` condition, but nothing publishes it RID-specific any more, so it is currently inert.
