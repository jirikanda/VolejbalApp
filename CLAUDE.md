# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

VolejbalApp is a Czech-language volleyball signup app (Blazor Server frontend + ASP.NET Core Web API backend) built on the HAVIT .NET framework stack. Solution file is `VolejbalApp.slnx`, targeting **.NET 10** (`net10.0`). Identifiers and domain language are Czech (Termín = scheduled session, Přihláška = signup, Osoba = person, Vzkaz = bulletin-board message).

## Common commands

All commands run from repo root unless noted.

```powershell
# Restore / build / test (mirrors .github/workflows/build.yml)
dotnet restore VolejbalApp.slnx
dotnet build VolejbalApp.slnx --configuration Release
dotnet test Tests/Tests.csproj --configuration Release

# Run a single test (uses Microsoft Testing Platform — see global.json)
dotnet test Tests/Tests.csproj --filter "FullyQualifiedName~VolejbalDbContext_CheckModelConventions"

# Publish (matches build pipeline)
dotnet publish Web/Web.csproj    --configuration Release --output ./publish/Web
dotnet publish WebAPI/WebAPI.csproj --configuration Release --output ./publish/WebAPI

# Regenerate Repositories / DataSources / Metadata from Model/* entities
# (writes into Model/_generated and DataLayer/_generated)
./DataLayer/Run-CodeGenerator.ps1
# or:  dotnet tool restore && dotnet efcodegenerator   (run with DataLayer as cwd)

# EF Core migrations — point tooling at Entity project, startup is Entity
dotnet ef migrations add <Name> --project Entity --startup-project Entity
dotnet ef database update --project Entity --startup-project Entity
```

The test runner is **Microsoft Testing Platform** (`EnableMSTestRunner=true` in `Directory.Build.props`, plus `global.json`), so test projects are `OutputType=Exe` and can also be launched directly: `Tests/bin/Release/net10.0/KandaEu.Volejbal.Tests.exe`.

`Release` builds set `TreatWarningsAsErrors=true` — warnings that build locally in Debug may break CI.

## Architecture

### Layering (referenced top → down)

```
Web (Blazor Server)  ──HTTP──►  WebAPI (REST)
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

`Web` does **not** reference Facades or below — it only references `Contracts`. It talks to `WebAPI` over HTTP via NSwag-generated typed clients (see [Web/Web.csproj](Web/Web.csproj) `OpenApiProjectReference`; clients live in the `KandaEu.Volejbal.Web.WebApiClients` namespace and are wired in [Web/Program.cs](Web/Program.cs) via `AddCustomizedHttpClient`). DTOs are shared by referencing `Contracts` from both sides (NSwag is configured with `GenerateDtoTypes:false`).

### HAVIT framework conventions (this is the biggest "you can't tell from the code" thing)

The repo is built on HAVIT's EF Core stack and conventions. Read these before changing data-access code:

- **Entities live in `Model/`** as plain POCOs. New entities go here.
- **`DataLayer/_generated/` and `Model/_generated/` are generated**. Do not hand-edit. After adding/changing an entity or `EntityConfiguration`, re-run `Run-CodeGenerator.ps1`. The generator produces: `IXxxRepository`/`XxxDbRepository` (+ Base + QueryProvider), `IXxxDataSource`/`XxxDbDataSource`, fakes, and `Model/_generated/Metadata/*`.
- **Hand-written repository extensions** sit in `DataLayer/Repositories/` (e.g. `OsobaDbRepository.cs`) — the generator emits `partial` bases so you can extend without touching generated code.
- **DataSources** (`IXxxDataSource.Data`, `.DataIncludingDeleted`) are the read-only `IQueryable` projection surface used by Facades. Repositories are write/lookup; DataSources are query.
- **`IUnitOfWork`** is the commit boundary — `AddForInsert`/`AddForUpdate`/`AddForDelete` then `CommitAsync`. See [Facades/Nastenka/NastenkaFacade.cs](Facades/Nastenka/NastenkaFacade.cs) for a canonical example.
- **DI registration is attribute-driven.** Mark a class `[Service]` (from `Havit.Extensions.DependencyInjection.Abstractions`) and it is picked up by `AddByServiceAttribute` in [DependencyInjection/ServiceCollectionExtensions.cs](DependencyInjection/ServiceCollectionExtensions.cs). Scoped lifetime is the default. Services targeted only at WebAPI use the `WebAPI` profile (see [Services/Infrastructure/ServiceProfiles.cs](Services/Infrastructure/ServiceProfiles.cs)).
- **Soft delete is a HAVIT convention.** Entities expose `Deleted` (DateTime?) and the generated `IXxxDataSource.Data` filters it out; use `DataIncludingDeleted` when you need both.

### WebAPI composition

- Single composition root is [DependencyInjection/ServiceCollectionExtensions.cs](DependencyInjection/ServiceCollectionExtensions.cs) via `ConfigureForWebAPI` (production) or `ConfigureForTests` (tests). Both call `ConfigureForAll` which installs EF Core (SQL Server, or in-memory for tests), HAVIT services, Hangfire, and runs `AddByServiceAttribute` across `DataLayer`, `Services`, `Facades` assemblies.
- [WebAPI/Startup.cs](WebAPI/Startup.cs) wires MVC, OpenAPI/NSwag, CORS, rate limiting (`DefaultAPI` policy = 10/5s window with 10 queued), Application Insights, exception monitoring (HAVIT), and Hangfire dashboard at `/hangfire`.
- **Hangfire uses in-memory storage** (`UseInMemoryStorage()` — see SQL Server config is commented out in `ServiceCollectionExtensions.InstallHangfire`). Jobs do not survive restart. `AutomaticRetryAttribute { Attempts = 0 }` — failed jobs are not retried.
- **On startup**: `DatabaseMigrationHostedService` runs `Database.MigrateAsync()` + seeds `CoreProfile`, and `EnsureTerminyStartupService` materializes upcoming Termíny. Both are gated on having a non-empty `ConnectionStrings:Database` (so OpenAPI/NSwag tooling can run without a DB).

### Web (Blazor Server)

- Interactive Server render mode, Czech locale forced via `UseRequestLocalization("cs-CZ")` in [Web/Program.cs](Web/Program.cs).
- UI components: **Havit.Blazor.Components.Web.Bootstrap** (Hx* components). Use these instead of writing raw Bootstrap markup where one fits.
- Web ↔ WebAPI URL comes from `ConnectionStrings:WebAPI` (Development default: `https://localhost:44398/`).

### Tests

Two test projects, deliberately separated:

- **`Tests/`** — CI tests (run in `dotnet test` step of `build.yml`). Currently a minimal model-convention check. Uses MSTest + EF Core InMemory.
- **`TestsForLocalDebugging/`** — local-only debugging tests that hit LocalDB (`(localdb)\mssqllocaldb`). Base class [TestsForLocalDebugging/TestBase.cs](TestsForLocalDebugging/TestBase.cs) wires up the real DI container via `ConfigureForTests`, with `EnsureDeleted` + `Migrate` + seed between tests. Do not add these to CI.

## Configuration

- WebAPI: `appsettings.WebAPI.json` + environment override + (Debug only) `*.local.json` (gitignored) + env vars + Azure Key Vault (URI from `ConnectionStrings:AzureKeyVault`). See [DependencyInjection/Configuration/KeyVaultConfig.cs](DependencyInjection/Configuration/KeyVaultConfig.cs).
- Web: `appsettings.Web.json` + environment override + env vars. No Key Vault on the Web side.
- The Entity project has its own `appsettings.json` (used by `VolejbalDesignTimeDbContextFactory` for EF tooling and the code generator).

## Build / coding conventions ([Directory.Build.props](Directory.Build.props) + [.editorconfig](.editorconfig))

- `Nullable` is **disabled** project-wide. Don't sprinkle `?`/`!` expecting NRT semantics.
- `ImplicitUsings` enabled; common HAVIT/EF usings are pulled in via per-project `GlobalUsings.cs`.
- `DisableTransitiveProjectReferences=true` — if you need a type from a non-direct dependency, add the explicit `ProjectReference`.
- Central package versions in [Directory.Packages.props](Directory.Packages.props) (`ManagePackageVersionsCentrally=true`). Add new packages by `<PackageVersion>` here, then `<PackageReference>` (no version) in the csproj.
- `.editorconfig` enforces tabs, file-scoped namespaces, usings outside namespace, `var` only when type is apparent, braces required. Style violations are warnings; `Release` turns them into errors.

## CI / deployment

- [.github/workflows/build.yml](.github/workflows/build.yml) — runs on push/PR to `master`: restore → build (Release) → test → publish Web + WebAPI as artifacts.