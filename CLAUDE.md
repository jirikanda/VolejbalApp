# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

VolejbalApp is a Czech-language volleyball signup app built on the HAVIT .NET framework stack. It is deployed as **two separate applications**: `Api` (Azure Functions, isolated worker — the REST API) and `Web.Client` (Blazor WebAssembly SPA on Azure Static Web Apps). Solution file is `VolejbalApp.slnx`, targeting **.NET 10** (`net10.0`). Identifiers and domain language are Czech (Termín = scheduled session, Přihláška = signup, Osoba = person, Vzkaz = bulletin-board message).

## Common commands

All commands run from repo root unless noted.

```powershell
# Restore / build / test (mirrors .github/workflows/build.yml)
dotnet restore VolejbalApp.slnx
dotnet build VolejbalApp.slnx --configuration Release
dotnet test src/Tests/Tests.csproj --configuration Release

# Run a single test (uses Microsoft Testing Platform — see global.json)
dotnet test src/Tests/Tests.csproj --filter "FullyQualifiedName~VolejbalDbContext_CheckModelConventions"

# Run the API locally (needs Azure Functions Core Tools + Azurite running for AzureWebJobsStorage)
dotnet run --project src/Api          # Azure.Functions.Sdk starts the Functions host; listens on :7071

# Publish Api (zip of this output is what gets deployed)
dotnet publish src/Api/Api.csproj --configuration Release --runtime linux-x64 --self-contained false --output ./publish/Api

# Apply EF migrations + run data seeds (deployment-time; also needed before first local run)
dotnet run --project src/MigrationTool -- --connectionstring "Data Source=(localdb)\mssqllocaldb;Initial Catalog=VolejbalApp;Application Name=VolejbalApp-MigrationTool;Trust Server Certificate=True"

# Regenerate Repositories / DataSources / Metadata from Model/* entities
# (writes into Model/_generated and DataLayer/_generated)
./src/DataLayer/Run-CodeGenerator.ps1
# or:  dotnet tool restore && dotnet efcodegenerator   (run with src/DataLayer as cwd)

# EF Core migrations — point tooling at Entity project, startup is Entity
dotnet ef migrations add <Name> --project src/Entity --startup-project src/Entity
dotnet ef database update --project src/Entity --startup-project src/Entity
```

There is **no API client generation step** — see *Contracts* below. API clients are produced by the Refit source generator at build time.

The test runner is **Microsoft Testing Platform** (`EnableMSTestRunner=true` in `Directory.Build.props`, plus `global.json`), so test projects are `OutputType=Exe` and can also be launched directly: `src/Tests/bin/Release/net10.0/KandaEu.Volejbal.Tests.exe`.

`Release` builds set `TreatWarningsAsErrors=true` — warnings that build locally in Debug may break CI.

## Architecture

### Layering (referenced top → down)

```
Web.Client (Blazor WASM on Static Web Apps)  ──HTTP──►  Api (Azure Functions, isolated worker)
                                                        │
                                                        ▼
                                                Facades  ── Contracts (DTOs + I*Api contracts, implemented by the facades)
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

`Web.Client` does **not** reference Facades or below — it only references `Contracts`. The two apps are deployed independently and to different hosts, so the client learns the API address from `wwwroot/appsettings.json` (`ApiBaseUrl`) and the API allows its origin through **platform CORS** configured in `infra/main.bicep`.

### The shared API contract (this is the least obvious thing in the repo)

`Contracts` holds one interface per API area — `ITerminApi`, `IOsobaApi`, `INastenkaApi`, `IPrihlaskaApi`, `IReportOsobApi`, `IReportTerminuApi`, `IDataSeedApi` — carrying Refit HTTP attributes. **The same interface is both the client contract and the server contract**:

- **Client**: `Web.Client` calls `AddRefitGeneratedClient<ITerminApi>(...)` ([App_Start/ApiClientConfig.cs](src/Web.Client/App_Start/ApiClientConfig.cs)); the Refit source generator emits the implementation at build time. Nothing is committed.
- **Server**: the facade implements it directly (`public class TerminFacade(...) : ITerminApi`), so the facade is checked against the client's expectations **by the compiler**. There are no separate `I*Facade` interfaces — the facade *is* the API surface.

Consequences to respect:

- **Refit requires an HTTP attribute on every method of an interface it generates.** So a facade method that must *not* be an endpoint cannot simply be added to `I*Api` — the generator breaks on it. When that day comes, reintroduce a derived interface for that one area (`public interface ITerminFacade : ITerminApi`, facade implements it, the function injects it) and put the unexposed method there. Don't do it pre-emptively for all areas: it was tried, and the only effect was seven empty files plus a duplicate generated Refit client for each (the `[ModuleInitializer]` carries `[DynamicDependency(All)]`, so trimming does not remove them — `Contracts` was 26 % larger).
- **`Refit` must stay referenced from `Contracts`** — the generator runs in the assembly where the interface lives.
- Routes live once, in [Contracts/Api/ApiRoutes.cs](src/Contracts/Api/ApiRoutes.cs), shared by the Refit attribute (`[Get("/" + ApiRoutes.Terminy)]`) and the HTTP trigger (`Route = ApiRoutes.Terminy`). **`host.json` therefore sets `routePrefix` to empty** and routes are written including `api/`. Do not reintroduce a prefix — it would silently double it on one side only.
- Route constraints (`{osobaId:int}`) must **not** appear in those constants: Refit does not understand them.
- [Tests/Api/ApiContractTests.cs](src/Tests/Api/ApiContractTests.cs) pairs every contract method with an HTTP trigger by route + verb (in both directions). It is the one thing the compiler cannot check.
- **Register with `AddRefitGeneratedClient<T>`, never `AddRefitClient<T>`.** Since Refit 14 the reflection request builder lives in a separate `Refit.Reflection` package and `AddRefitClient` demands it — without it the call throws `NotSupportedException` (*"This interface needs the reflection request builder"*) **at runtime, not at build time**, so the compiler will not warn you. The generated variant also avoids reflection entirely, which is what a trimmed WebAssembly build needs.
- **Refit must stay at ≥ 15.0.0.** Interfaces in a referenced library used from Blazor WASM were broken before that ([refit#2287](https://github.com/reactiveui/refit/issues/2287)): the generator registers clients from a `[ModuleInitializer]`, which Mono does not run eagerly, so resolution failed with *"doesn't look like a Refit interface"*.

### HAVIT framework conventions (the other thing you can't tell from the code)

The repo is built on HAVIT's EF Core stack and conventions. Read these before changing data-access code:

- **Entities live in `Model/`** as plain POCOs. New entities go here.
- **`DataLayer/_generated/` and `Model/_generated/` are generated**. Do not hand-edit. After adding/changing an entity or `EntityConfiguration`, re-run `Run-CodeGenerator.ps1`. The generator produces: `IXxxRepository`/`XxxDbRepository` (+ Base + QueryProvider), `IXxxDataSource`/`XxxDbDataSource`, fakes, and `Model/_generated/Metadata/*`.
- **Hand-written repository extensions** sit in `DataLayer/Repositories/` (e.g. `OsobaDbRepository.cs`) — the generator emits `partial` bases so you can extend without touching generated code.
- **DataSources** (`IXxxDataSource.Data`, `.DataIncludingDeleted`) are the read-only `IQueryable` projection surface used by Facades. Repositories are write/lookup; DataSources are query.
- **`IUnitOfWork`** is the commit boundary — `AddForInsert`/`AddForUpdate`/`AddForDelete` then `CommitAsync`. See [Facades/Nastenka/NastenkaFacade.cs](src/Facades/Nastenka/NastenkaFacade.cs) for a canonical example.
- **DI registration is attribute-driven.** Mark a class `[Service]` (from `Havit.Extensions.DependencyInjection.Abstractions`) and it is picked up by `AddByServiceAttribute` in [DependencyInjection/ServiceCollectionExtensions.cs](src/DependencyInjection/ServiceCollectionExtensions.cs). Scoped lifetime is the default.
- **The facades must say `[Service(ServiceType = typeof(I*Api))]` explicitly.** Bare `[Service]` resolves the service type by the `XxxFacade` → `IXxxFacade` naming convention; since the facades now implement `I*Api` (see *shared API contract* above), that lookup finds nothing and **the worker dies at startup** with `InvalidOperationException: Type …TerminFacade implements no interface to register`. This is invisible to both the compiler and `dotnet test` — the tests never build the production DI container — so it only shows up as `0 functions found` plus a language-worker restart loop in the deployed app. After touching facade interfaces or their `[Service]` attributes, **run the app** (`dotnet run --project src/Api`), don't just build it.
- **Soft delete is a HAVIT convention.** Entities expose `Deleted` (DateTime?) and the generated `IXxxDataSource.Data` filters it out; use `DataIncludingDeleted` when you need both.
- **Never use `DateTime.Now` in server code.** Time comes from `ITimeService`, implemented by [Services/Infrastructure/TimeService/ApplicationTimeService.cs](src/Services/Infrastructure/TimeService/ApplicationTimeService.cs), which pins the zone to `Europe/Prague` in code. Flex Consumption ignores `TZ` and `WEBSITE_TIME_ZONE`, so the process runs in UTC — the explicit zone is the only thing keeping dates right.

### Api (Azure Functions) composition

- Single composition root is [DependencyInjection/ServiceCollectionExtensions.cs](src/DependencyInjection/ServiceCollectionExtensions.cs) via `ConfigureForWebAPI` (production) or `ConfigureForTests` (`TestsForLocalDebugging`) — the method name is kept from the pre-merge era, do not rename it casually. Both call `ConfigureForAll`, which installs EF Core (SQL Server; there is no in-memory variant — the CI `Tests` project builds its own `DbContextOptions` and never touches this composition root), HAVIT services, and runs `AddByServiceAttribute` across the `Services` and `Facades` assemblies. **`DataLayer` is deliberately not scanned** — it holds no `[Service]` and does not even reference the attribute package; its repositories, DataSources and data seeds come from the generated `AddDataLayerServices()`. Everything registers on `ServiceAttribute.DefaultProfile`; the `WebAPI` profile from the pre-merge era is gone, because no class ever carried it.
- [Api/Program.cs](src/Api/Program.cs) uses `FunctionsApplication.CreateBuilder(args)` + `ConfigureFunctionsWebApplication()` (**ASP.NET Core integration**, so functions take `HttpRequest` and return `IActionResult`), loads `appsettings.Api.json`, sets the `cs-CZ` culture, and registers Application Insights for the worker.
- **Function discovery is source-generated, not reflective.** `Microsoft.Azure.Functions.Worker.Sdk.Generators` runs two generators at compile time: `FunctionMetadataProviderGenerator` bakes every function's route, verb and auth level into `GeneratedFunctionMetadataProvider`, and `FunctionExecutorGenerator` emits `DirectFunctionExecutor`, which dispatches on `EntryPoint` and calls the method directly — there is no `MethodInfo.Invoke` and no assembly scanning (the only reflection left is a `Type.GetType` lookup per function class, for DI activation). At startup the worker just hands that prebuilt list to the host, which is why the publish output has no `functions.metadata` file. Practical consequence: `[HttpTrigger]` routes must be **compile-time constants** — they cannot be computed or read from configuration.
- **The ASP.NET Core integration does not give you the ASP.NET Core middleware pipeline or routing.** There is no `MapControllers`, no MVC filters, no `UseCors`, no `UseRateLimiter`, no `UseRequestLocalization`, no `Microsoft.AspNetCore.OpenApi`. Anything of that sort has to be solved another way — see the table in `infra/README.md` and the replacements in [Api/Infrastructure/](src/Api/Infrastructure/).
- **Errors** are mapped by [ExceptionHandlingMiddleware](src/Api/Infrastructure/ExceptionHandlingMiddleware.cs) (`IFunctionsWorkerMiddleware`): `SecurityException` → 403, `OperationFailedException`/`ObjectNotFoundException` → 422, everything else → 500 + `IExceptionMonitoringService`. It sets the response through `context.GetInvocationResult().Value`, **not** by writing to `HttpResponse` after the call — that is documented as unreliable with the ASP.NET Core integration.
- **Request bodies** are read and validated by [RequestBodyReader](src/Api/Infrastructure/RequestBodyReader.cs) (DataAnnotations → 422 + `ValidationErrorModel`). This replaces what `[ApiController]` + `ValidateModelAttribute` used to do.
- **There is no background work at all** — no Timer trigger, no in-process scheduler, nothing that constrains the app to a single instance. The one recurring task that existed (topping the schedule up to three future `Termin`y, hourly) is now done **lazily on read**: [TerminFacade.GetTerminyAsync](src/Facades/Terminy/TerminFacade.cs) counts what it got and calls [EnsureTerminyService](src/Services/Terminy/EnsureTerminy/EnsureTerminyService.cs) when it is short, then re-reads. **Concurrency is handled by the database, not by a lock**: the unique index `UIDX_Termin_Datum_Deleted` rejects a duplicate date, the loser's `CommitAsync` throws `DbUpdateException`, and the service does `IUnitOfWork.Clear()` + retry (3 attempts) — on the retry there is usually nothing left to create. This works only because the date to insert is deterministic (same weekday + 7 days), so parallel runs race for the *same* row rather than creating different ones. Don't reintroduce a timer, and don't drop that unique index. `IDeaktivaceOsobJob` / [Services/DeaktivaceOsob/DeaktivaceOsobService.cs](src/Services/DeaktivaceOsob/DeaktivaceOsobService.cs) still exist and stay DI-registered but are **deliberately never scheduled** — deactivation is a manual action in the player-management screen. Don't "clean up" that unused job; it is kept on purpose.
- **The app does NOT migrate or seed the database at runtime.** Schema migrations + data seeds run via the **`MigrationTool`** console project at deployment time (see below) — the app assumes the schema already exists.
- `/api/health` ([Api/Functions/HealthFunctions.cs](src/Api/Functions/HealthFunctions.cs)) registers no checks on purpose — it answers 200 as soon as the app stands. Flex Consumption has no health probes (unlike the previous ACA hosting), so it serves manual checks and cold-start measurement only.
- **There is no rate limiting.** The old `DefaultAPI` limiter (10 req / 5 s) was a blunt guard for a single replica and its database; its role is now played by `maximumInstanceCount` in the bicep template.
- **There is no OpenAPI document and no Scalar UI.** Azure Functions cannot export one at build time (`Microsoft.Extensions.ApiDescription.Server` is tied to the ASP.NET Core pipeline), and the official `Microsoft.Azure.Functions.Worker.Extensions.OpenApi` is in maintenance mode and broken on .NET 10. Clients don't need it — the contract is the interface.

### MigrationTool (deployment-time migrations + seeds)

- Console app ([MigrationTool/Program.cs](src/MigrationTool/Program.cs)): applies EF Core migrations and runs `CoreProfile` data seeds, then exits. **CI/CD does not touch it** — it is neither published nor run by any workflow; it is a manual step from a local clone, both for production and for local dev.
- Wiring: `ConfigureForMigrationTool` in [DependencyInjection/ServiceCollectionExtensions.cs](src/DependencyInjection/ServiceCollectionExtensions.cs) (deliberately minimal — EF Core + DataLayer + [Services/Infrastructure/MigrationTool/MigrationService.cs](src/Services/Infrastructure/MigrationTool/MigrationService.cs); no Services/Facades).
- Parameters: `--connectionstring <cs>` (maps to `ConnectionStrings:Database`), optional `--commandtimeout <seconds>` (default 300); environment variables work too (`ConnectionStrings__Database`).
- **Local dev**: after cloning or adding a migration, run the MigrationTool (F5 profile uses LocalDB). `TestsForLocalDebugging` is unaffected (it migrates+seeds itself per test).

### Web.Client (Blazor WebAssembly)

- Standalone WASM app on Azure Static Web Apps; SPA fallback is in `wwwroot/staticwebapp.config.json`.
- The API base address comes from `wwwroot/appsettings.json` (`ApiBaseUrl`) — **it is a committed production value** and must match the Function App URL that `infra/main.bicep` outputs. `appsettings.Development.json` points at `http://localhost:7071`.
- Czech locale is set in `Program.cs` (`CultureInfo.DefaultThreadCurrentCulture`); the csproj sets `BlazorWebAssemblyLoadAllGlobalizationData=true` because Czech is not in the default ICU shards — don't remove it or date formatting breaks.
- UI components: **Havit.Blazor.Components.Web.Bootstrap** (Hx* components). Use these instead of writing raw Bootstrap markup where one fits. Local state via `Havit.Blazor.Storage` (`ILocalStorageService`).

### Tests

Two test projects, deliberately separated:

- **`Tests/`** — CI tests (run in `dotnet test` step of `build.yml`). A model-convention check plus the API contract tests described above. Uses MSTest + EF Core InMemory.
- **`TestsForLocalDebugging/`** — local-only debugging tests that hit LocalDB (`(localdb)\mssqllocaldb`). Base class [TestsForLocalDebugging/TestBase.cs](src/TestsForLocalDebugging/TestBase.cs) wires up the real DI container via `ConfigureForTests`, with `EnsureDeleted` + `Migrate` + seed between tests — unconditionally, LocalDB is the only option here (`EnsureTerminyServiceTests` needs a real unique index, which the in-memory provider does not enforce). Do not add these to CI.

## Configuration

- Api: `appsettings.Api.json` + environment override + (Debug only) `*.local.json` (gitignored) + env vars. `local.settings.json` (gitignored) holds the local Functions host settings — Azurite connection and dev CORS. Secrets (connection string, App Insights) are supplied as app settings from the bicep template in production — no Key Vault.
- Web.Client: `wwwroot/appsettings.json` (see above).
- The Entity project has its own `appsettings.json` for EF tooling and the code generator. It is `CopyToPublishDirectory=Never` **on purpose** — hosts load `appsettings.json` as their first configuration layer, so shipping it would put a LocalDB connection string into the deployed package.

## Build / coding conventions ([Directory.Build.props](Directory.Build.props) + [.editorconfig](.editorconfig))

- `Nullable` is **disabled** project-wide. Don't sprinkle `?`/`!` expecting NRT semantics.
- `ImplicitUsings` enabled; common HAVIT/EF usings are pulled in via per-project `GlobalUsings.cs`.
- `DisableTransitiveProjectReferences=true` — if you need a type from a non-direct dependency, add the explicit `ProjectReference`.
- Central package versions in [Directory.Packages.props](Directory.Packages.props) (`ManagePackageVersionsCentrally=true`). Add new packages by `<PackageVersion>` here, then `<PackageReference>` (no version) in the csproj. Application Insights is deliberately held at **2.x** (`ConfigureTelemetryModule` was removed in 3.x) and dependabot ignores `Microsoft.ApplicationInsights*`.
- `Api.csproj` uses the **`Azure.Functions.Sdk` project SDK**, not `Microsoft.NET.Sdk`; its version is pinned in [global.json](global.json) under `msbuild-sdks`. Do not add `OutputType`, `AzureFunctionsVersion` or `FunctionsEnableWorkerIndexing` — the SDK sets them and complains (`AZFW0110`/`AZFW0111`).
- **`AZFW0108`**: a solution-level `dotnet restore` does not run the SDK's restore hook for the generated Functions extensions project, and `func start` invalidates its marker. Both CI workflows therefore run `dotnet restore src/Api/Api.csproj` as a separate step **and then build with `--no-restore`** — a build without that flag does its own solution-level restore, which invalidates the marker again and brings the warning back. Locally: `dotnet restore src/Api/Api.csproj` once, then `dotnet build … --no-restore`.
- [Api/Functions/.editorconfig](src/Api/Functions/.editorconfig) turns off `IDE0060` for that folder only: the `[HttpTrigger]` parameter is required by the Functions binding contract even when the body never reads it, which would otherwise fail the Release build.
- `.editorconfig` enforces tabs, file-scoped namespaces, usings outside namespace (`System.*` first), **explicit types over `var` (never use `var`)**, required braces, and parentheses-for-clarity in binary/relational expressions. Style violations are warnings; `Release` turns them into errors.
- Project-specific naming (full detail in the `CodeConventions` skill, [.claude/skills/CodeConventions/SKILL.md](.claude/skills/CodeConventions/SKILL.md)): instance fields `_camelCase`, static fields `s_camelCase`, **primary-constructor parameters also `_camelCase`** (e.g. `MailingService(IOptions<MailingOptions> _mailingOptions)`), fields are always `private`, async methods end with `Async` and take `CancellationToken` as the last parameter.

## CI / deployment

- [.github/workflows/build.yml](.github/workflows/build.yml) — CI for `master` and PRs into `master`: restore → build (Release) → test. Nothing is published from here.
- [.github/workflows/deploy.yml](.github/workflows/deploy.yml) — **manual only** (`workflow_dispatch`, run against `master`); the whole path to production in four jobs, split by *what* they deploy:

  ```
  build ──► infrastructure ──┬──► deploy-api
                             └──► deploy-frontend
  ```

  `build`: restore → build → test → publish the Api for `linux-x64` → zip, and publish `Web.Client`; both go up as artifacts, so nothing is compiled twice. `infrastructure`: OIDC login → `az deployment group create` against [infra/main.bicep](infra/main.bicep) → reads the template outputs once and re-exposes them as **job outputs**. `deploy-api` and `deploy-frontend` then run **in parallel** over the finished infrastructure — the API package via `Azure/functions-action` (`sku: flexconsumption`), the client via `Azure/static-web-apps-deploy` with a deployment token read at run time through the same OIDC session. There is **no version input** — triggering the workflow manually *is* the decision of what goes to production.
- **Why infrastructure is its own job**: it is the only part that changes the shape of the environment, and both app deployments depend on it — the Function App must exist (and its identity must hold the storage roles) before a package is pushed to it, and the SWA name comes from the template outputs. Splitting the two app deployments off means a failure in one does not block the other, and the log says which half broke.
- **`environment: Production` must be on every job that logs into Azure** — the federated credential's subject is bound to the environment, not to the job name. Consequence of the split: if required reviewers are ever configured on `Production`, all three deploy jobs will be approved separately (today the environment has no protection rules).
- The deploy job applies the **whole template**, not just the app — ARM's incremental mode no-ops the unchanged resources, and in exchange the infrastructure cannot drift from `main.bicep`.
- **Azure authentication is OIDC / federated credentials**, no stored password: `permissions: id-token: write` plus `client-id`/`tenant-id`/`subscription-id` on `azure/login`, which come from repository **variables** (`vars.AZURE_*`, not `secrets.*` — they are identifiers, not secrets; using the wrong context yields an empty string and an opaque login failure). The federated credential's subject must be `repo:jirikanda@5111719/VolejbalApp@169379851:environment:Production` — two separate traps in one string: it is an **environment** subject (the job targets `environment: Production`, so a ref-based subject silently fails to match), and the repo has **immutable subject claims** enabled, so the prefix carries owner and repo IDs rather than just names. Check the live prefix with `gh api repos/jirikanda/VolejbalApp/actions/oidc/customization/sub`. `DATABASE_CONNECTION_STRING` is the only real secret.
- [infra/main.bicep](infra/main.bicep) (see [infra/README.md](infra/README.md) for secrets and one-time setup) creates the Log Analytics workspace, Application Insights (workspace-based on it), the storage account + deployment container, the Flex Consumption plan (`FC1`), the Function App, its storage role assignments, and the Static Web App — **the database and the DNS record are out of its scope**. Schema migrations are **not** touched by the pipeline at all; run `MigrationTool` by hand against production *before* triggering a deploy that needs a new migration.
- **Cold start is the reason this app is on Functions at all.** Measured on the previous ACA hosting with scale-to-zero: **33 s** to first byte (much of it pulling the image from ghcr.io, outside Azure). `PublishReadyToRun` in [Api.csproj](src/Api/Api.csproj) is conditioned on `RuntimeIdentifier != ''` and is the single biggest lever left — passing `-p:PublishReadyToRun=true` on the CLI instead would flow into `Web.Client` (browser-wasm) and fail with NETSDK1095. Similarly, never `dotnet restore` the whole solution with `--runtime`. If cold start still hurts, the escape hatch is `alwaysReadyInstanceCount` in the bicep template (~$6.50/month, no free grant) — measure before turning it on.
