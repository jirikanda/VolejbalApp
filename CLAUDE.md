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

# Publish (matches build pipeline; the WASM client is bundled into the host output automatically)
dotnet publish Web/Web.csproj --configuration Release --output ./publish/Web

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
Web.Client (Blazor WASM, runs in browser)  ──HTTP──►  Web (host: serves the client + REST API + Hangfire)
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

- Single composition root is [DependencyInjection/ServiceCollectionExtensions.cs](DependencyInjection/ServiceCollectionExtensions.cs) via `ConfigureForWebAPI` (production) or `ConfigureForTests` (tests) — the method name and the `WebAPI` service profile string are kept from the pre-merge era, do not rename them casually (the profile is referenced by `[Service(Profile = ...)]` attributes). Both call `ConfigureForAll` which installs EF Core (SQL Server, or in-memory for tests), HAVIT services, Hangfire, and runs `AddByServiceAttribute` across `DataLayer`, `Services`, `Facades` assemblies.
- [Web/Startup.cs](Web/Startup.cs) wires MVC controllers, Razor Components (`AddInteractiveWebAssemblyComponents`), OpenAPI/NSwag, rate limiting (`DefaultAPI` policy = 10/5s window with 10 queued — applied **only** to `MapControllers()`, not to Blazor/static assets), Application Insights, exception monitoring (HAVIT), and Hangfire dashboard at `/hangfire`.
- Blazor endpoints: `MapRazorComponents<App>().AddInteractiveWebAssemblyRenderMode().AddAdditionalAssemblies(...)` with the root document at [Web/Components/App.razor](Web/Components/App.razor). **Prerendering is off** (`new InteractiveWebAssemblyRenderMode(prerender: false)`) — the host renders only the static shell, all UI runs client-side; the host therefore does not register any client services.
- In Development, `DelayRequestMiddleware` (artificial 500 ms) applies **only under `/api`** so it doesn't slow WASM asset downloads; `UseWebAssemblyDebugging()` is enabled.
- **Hangfire uses in-memory storage** (`UseInMemoryStorage()` — see SQL Server config is commented out in `ServiceCollectionExtensions.InstallHangfire`). Jobs do not survive restart. `AutomaticRetryAttribute { Attempts = 0 }` — failed jobs are not retried.
- **On startup**: `DatabaseMigrationHostedService` runs `Database.MigrateAsync()` + seeds `CoreProfile`, and `EnsureTerminyStartupService` materializes upcoming Termíny. Both are gated on having a non-empty `ConnectionStrings:Database` (so OpenAPI/NSwag tooling can run without a DB).

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

- Web (host): `appsettings.Web.json` + environment override + (Debug only) `*.local.json` (gitignored) + env vars + Azure Key Vault (URI from `ConnectionStrings:AzureKeyVault`). See [DependencyInjection/Configuration/KeyVaultConfig.cs](DependencyInjection/Configuration/KeyVaultConfig.cs).
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

- [.github/workflows/build.yml](.github/workflows/build.yml) — runs on push/PR to `master`: restore → build (Release) → test → publish `Web` (single artifact; the WASM client is bundled into the host's `wwwroot/_framework`).