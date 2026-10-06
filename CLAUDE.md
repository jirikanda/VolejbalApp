# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

VolejbalApp is a Czech-language volleyball signup app. It is deployed as **two separate applications**: `Api` (Azure Functions, isolated worker — the REST API) and `Web.Client` (Blazor WebAssembly SPA on Azure Static Web Apps). Data lives in **Cosmos DB (free tier)**. Solution file is `VolejbalApp.slnx`, targeting **.NET 10** (`net10.0`). Identifiers and domain language are Czech (Termín = scheduled session, Přihláška = signup, Osoba = person, Vzkaz = bulletin-board message).

The app used to run on SQL Server with HAVIT's EF Core stack (repositories, DataSources, `IUnitOfWork`, code generator, migrations). All of it is gone — if you find a reference to `Havit.Data.*`, `VolejbalDbContext`, `_generated`, DataSources or data seeds anywhere, it is a leftover, not a pattern to follow.

## Common commands

All commands run from repo root unless noted.

```powershell
# Restore / build / test (mirrors .github/workflows/build.yml)
dotnet restore VolejbalApp.slnx
dotnet build VolejbalApp.slnx --configuration Release
dotnet test src/Tests/Tests.csproj --configuration Release

# Run a single test (uses Microsoft Testing Platform — see global.json)
dotnet test src/Tests/Tests.csproj --filter "FullyQualifiedName~CompositionRootTests"

# Run the API locally (needs Azure Functions Core Tools + Azurite for AzureWebJobsStorage
# + a running Cosmos DB Emulator on https://localhost:8081)
dotnet run --project src/Api          # Azure.Functions.Sdk starts the Functions host; listens on :7071

# Publish Api (zip of this output is what gets deployed)
dotnet publish src/Api/Api.csproj --configuration Release --runtime linux-x64 --self-contained false --output ./publish/Api

# Create the local database + containers in the emulator (needed once before the first local run;
# args default to the emulator via launchSettings, see MigrationTool below)
dotnet run --project src/MigrationTool
```

There is **no schema migration step and no code generation step**. Cosmos has no schema; the containers
are created by `infra/main.bicep` in Azure and by `MigrationTool` locally.

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
                                                Services (domain services, jobs)
                                                        │
                                                        ▼
                                                DataLayer  (Cosmos client + repositories)
                                                        │
                                                        ▼
                                                Model   (documents as plain POCOs)
```

`Web.Client` does **not** reference Facades or below — it only references `Contracts`. The two apps are deployed independently and to different hosts, so the client learns the API address from `wwwroot/appsettings.json` (`ApiBaseUrl`) and the API allows its origin through **platform CORS** configured in `infra/main.bicep`.

### The shared API contract (this is the least obvious thing in the repo)

`Contracts` holds one interface per API area — `ITerminApi`, `IOsobaApi`, `INastenkaApi`, `IPrihlaskaApi`, `IReportOsobApi`, `IReportTerminuApi` — carrying Refit HTTP attributes. **The same interface is both the client contract and the server contract**:

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

### The document model (the second least obvious thing)

Three containers in one shared-throughput database at the 400 RU/s minimum (the free-tier grant is 1000 RU/s; the rest is left free for another database on the same account, and `cosmosTotalThroughputLimit` in the bicep caps the account at the grant). The whole
dataset fits in a single physical partition and always will, so partition keys are not about scale: every
container uses `/id`, which keeps the model free of Cosmos-only properties and gives point reads and writes a
key that is always at hand.

| Container | Partition key | Document |
| --- | --- | --- |
| `osoby` | `/id` | one per person, `id` is a GUID |
| `terminy` | `/id`, i.e. the date itself | one per session, **`id` is the date** (`"2026-01-13"`), přihlášky embedded in a `prihlasky` array |
| `vzkazy` | `/id` | one per message, author referenced by id (names are looked up through `IOsobaRepository.GetOsobyAsync`) |

Things that will bite you if you don't know them:

- **`Termin.Id` is the date, and that is what enforces uniqueness.** The partition key is `/id`, so every termín is its own logical partition and Cosmos rejects a second document with the same date with a 409. This replaces the old `UIDX_Termin_Datum_Deleted`. Don't make the id a GUID "for consistency" — the concurrency design in `EnsureTerminyService` rests on it. A partition key derived from the date (the season was tried) buys nothing here: lists are read across seasons anyway, and a derived key would make old documents unreachable if the derivation rule ever changed.
- **Přihláška is not an entity, it is an array item** ([Model/Prihlaska.cs](src/Model/Prihlaska.cs)), **one per person and termín**: `deleted == null` means signed up, `deleted != null` is the odhláška tombstone the UI needs (`IsOdhlaseny`). Re-signing up flips it back. This replaces `UIDX_Prihlaska_TerminId_OsobaId_Deleted`.
- **Signing up is read-modify-write under an ETag**, with a retry loop in [PrihlaskaFacade.UpravTerminAsync](src/Facades/Prihlasky/PrihlaskaFacade.cs) — two people signing up for the same termín write the same document. `ITerminRepository.TryReplaceTerminAsync` returns `false` on 412 rather than throwing; the caller re-reads and repeats.
- **Null is never written to a document.** `DefaultIgnoreCondition = WhenWritingNull` in [CosmosClientFactory](src/DataLayer/Cosmos/CosmosClientFactory.cs), and every soft-delete query says `NOT IS_DEFINED(c.deleted)`. Write a null and the property becomes *defined* — the queries would silently stop matching.
- **The serializer must stay System.Text.Json.** The SDK's default is Newtonsoft with PascalCase, which would store `Id` as `"Id"` while Cosmos requires `"id"`. `Newtonsoft.Json` is still a `PackageReference` because the Cosmos SDK demands one (its own internal types), not because we serialize with it.
- **Cosmos sorts strings ordinally**, so `ORDER BY` over a name puts Čapek after Zeman. Names are sorted in memory through [DataLayer/OsobaRazeniExtensions.cs](src/DataLayer/OsobaRazeniExtensions.cs) (`OrderByPrijmeniJmeno`).
- **Every container is partitioned by `/id`, so all list queries run cross-partition** (each document is its own partition; the model carries no property that exists only for Cosmos). At this data volume the fan-out is free: the whole dataset sits in one physical partition, so a cross-partition query is served by that one partition anyway. Only point reads and writes use the key.
- **No multi-document writes exist, so no transactions are needed.** Every write touches exactly one document (a person, a message, or a termín with its embedded přihlášky), and that is what makes this model work. If you ever need to write two documents atomically, they must share a logical partition (`TransactionalBatch`); if they would live in two containers, the model is wrong, not the database.

Conventions that survived the move from SQL Server:

- **Entities live in `Model/`** as plain POCOs — now they are the document shape. New entities go here.
- **DI registration is attribute-driven.** Mark a class `[Service]` (from `Havit.Extensions.DependencyInjection.Abstractions`) and it is picked up by `AddByServiceAttribute` in [DependencyInjection/ServiceCollectionExtensions.cs](src/DependencyInjection/ServiceCollectionExtensions.cs). Scoped lifetime is the default. **DataLayer is the exception** — it registers itself in `AddDataLayerServices()`, all singletons (see below).
- **The facades must say `[Service(ServiceType = typeof(I*Api))]` explicitly.** Bare `[Service]` resolves the service type by the `XxxFacade` → `IXxxFacade` naming convention; since the facades implement `I*Api` (see *shared API contract* above), that lookup finds nothing and **the worker dies at startup** with `InvalidOperationException: Type …TerminFacade implements no interface to register`. The compiler cannot see this; it used to show up only as `0 functions found` plus a language-worker restart loop in the deployed app. [Tests/DependencyInjection/CompositionRootTests.cs](src/Tests/DependencyInjection/CompositionRootTests.cs) now builds the real production container and resolves every `I*Api`, so CI catches it — it needs no database, because `CosmosClient`'s constructor does no I/O.
- **Soft delete**: `Osoba` and `Termin` keep a `Deleted` (DateTime?) and every query filters it out. Deleting a person does not touch their přihlášky — the detail screen skips array items whose `osobaId` is not among the loaded people ([TerminFacade.GetDetailTerminuAsync](src/Facades/Terminy/TerminFacade.cs)). **A dangling reference to a person (vzkaz author, přihláška) cannot arise through the application**: the facades load the person before storing the reference, deletion is soft, and the SQL import carries every person over with deterministic ids. `IOsobaRepository.GetOsobyAsync` therefore throws on a missing id on purpose (the bulletin board and the statistics answer 422 until the document is fixed) — it can only mean someone edited the data by hand, and that should surface immediately rather than be papered over.
- **Never use `DateTime.Now` in server code.** Time comes from the BCL `TimeProvider`, registered as [Services/Infrastructure/Time/PragueTimeProvider.cs](src/Services/Infrastructure/Time/PragueTimeProvider.cs), which pins `LocalTimeZone` to `Europe/Prague` in code; call sites use the `GetLocalToday()` / `GetLocalDateTime()` extensions (wall-clock `DateTime`, no zone — the shape the documents store). Flex Consumption ignores `TZ` and `WEBSITE_TIME_ZONE`, so the process runs in UTC — the explicit zone is the only thing keeping dates right.

### Api (Azure Functions) composition

- Single composition root is [DependencyInjection/ServiceCollectionExtensions.cs](src/DependencyInjection/ServiceCollectionExtensions.cs) via `ConfigureForWebAPI` (production) or `ConfigureForTests` (`TestsForLocalDebugging`) — the method name is kept from the pre-merge era, do not rename it casually. Both call `ConfigureForAll`, which installs the DataLayer (Cosmos), HAVIT services, and runs `AddByServiceAttribute` across the `Services` and `Facades` assemblies. **`DataLayer` is deliberately not scanned** — it holds no `[Service]` and does not reference the attribute package; its client and repositories come from `AddDataLayerServices(CosmosOptions)`. Everything registers on `ServiceAttribute.DefaultProfile`; the `WebAPI` profile from the pre-merge era is gone, because no class ever carried it.
- **`CosmosClient` and the repositories are singletons**, registered in [DataLayer/ServiceCollectionExtensions.cs](src/DataLayer/ServiceCollectionExtensions.cs). The client is expensive to build and holds connections; registering it scoped (the `[Service]` default) would re-establish connections on every request and is the single most expensive mistake available on Flex Consumption. Connection mode is **Gateway** for the same reason — a short-lived 0.25-core instance never amortizes the direct TCP channel.
- [Api/Program.cs](src/Api/Program.cs) uses `FunctionsApplication.CreateBuilder(args)` + `ConfigureFunctionsWebApplication()` (**ASP.NET Core integration**, so functions take `HttpRequest` and return `IActionResult`), loads `appsettings.Api.json`, sets the `cs-CZ` culture, and registers Application Insights for the worker. **The worker's Application Insights runs with Live Metrics (QuickPulse), performance counters, event counters and the diagnostics module (heartbeat + IMDS instance metadata) switched off on purpose** — each of them starts threads or connections at process start, which a short-lived 0.25-core Flex Consumption instance never amortizes. Requests, dependencies (Cosmos calls) and exceptions are still collected. Don't re-enable them "for completeness"; if you need Live Metrics for a debugging session, turn it on temporarily.
- **Function discovery is source-generated, not reflective.** `Microsoft.Azure.Functions.Worker.Sdk.Generators` runs two generators at compile time: `FunctionMetadataProviderGenerator` bakes every function's route, verb and auth level into `GeneratedFunctionMetadataProvider`, and `FunctionExecutorGenerator` emits `DirectFunctionExecutor`, which dispatches on `EntryPoint` and calls the method directly — there is no `MethodInfo.Invoke` and no assembly scanning (the only reflection left is a `Type.GetType` lookup per function class, for DI activation). At startup the worker just hands that prebuilt list to the host, which is why the publish output has no `functions.metadata` file. Practical consequence: `[HttpTrigger]` routes must be **compile-time constants** — they cannot be computed or read from configuration.
- **The ASP.NET Core integration does not give you the ASP.NET Core middleware pipeline or routing.** There is no `MapControllers`, no MVC filters, no `UseCors`, no `UseRateLimiter`, no `UseRequestLocalization`, no `Microsoft.AspNetCore.OpenApi`. Anything of that sort has to be solved another way — see the table in `infra/README.md` and the replacements in [Api/Infrastructure/](src/Api/Infrastructure/).
- **Errors** are mapped by [ExceptionHandlingMiddleware](src/Api/Infrastructure/ExceptionHandlingMiddleware.cs) (`IFunctionsWorkerMiddleware`): `SecurityException` → 403, `OperationFailedException` → 422, everything else → 500 + `IExceptionMonitoringService`. **Only the handled exceptions send their `Message` to the client**; a 500 carries a fixed generic text, because the client renders the response body to the user and a foreign exception's message may leak internals (`CosmosException` embeds the account endpoint and the whole request diagnostics). "Not found" cases (unknown termín/osoba id from the URL) are thrown by the repositories as [DataLayer/ObjectNotFoundException](src/DataLayer/ObjectNotFoundException.cs) — our own type, the one from `Havit.Data.Patterns` left with the EF stack — and mapped to 422 as well, because for the client it is the same "reload, the data changed" situation; `GetOsobaAsync`/`GetTerminAsync` never return null. It sets the response through `context.GetInvocationResult().Value`, **not** by writing to `HttpResponse` after the call — that is documented as unreliable with the ASP.NET Core integration.
- **Request bodies** are read and validated by [RequestBodyReader](src/Api/Infrastructure/RequestBodyReader.cs) (DataAnnotations → 422 + `ValidationErrorModel`). This replaces what `[ApiController]` + `ValidateModelAttribute` used to do.
- **There is no background work at all** — no Timer trigger, no in-process scheduler, nothing that constrains the app to a single instance. The one recurring task that existed (topping the schedule up to three future `Termin`y, hourly) is now done **lazily on read**: [TerminFacade.GetTerminyAsync](src/Facades/Terminy/TerminFacade.cs) delegates to [EnsureTerminyService](src/Services/Terminy/EnsureTerminy/EnsureTerminyService.cs), which reads the future termíny once (including soft-deleted ones, so that the count and the date to continue from come from a single snapshot), creates what is missing and returns the resulting list — the facade never re-reads. **Concurrency is handled by the database, not by a lock**: the termín's `id` *is* its date, so Cosmos rejects the duplicate with a 409, `ITerminRepository.TryCreateTerminAsync` turns that into `false` (the Cosmos SDK stays confined to DataLayer) and the loser retries (3 attempts) — on the retry there is usually nothing left to create. This works only because the date to insert is deterministic (same weekday + 7 days), so parallel runs race for the *same* document rather than creating different ones. The date arithmetic itself lives in [TerminDatumGeneratorService](src/Services/Terminy/EnsureTerminy/TerminDatumGeneratorService.cs) (a `[Service]` without dependencies — the date is passed in, so it is deterministic and unit-tested in CI). Don't reintroduce a timer, and don't make the termín id a GUID. The hourly deactivation job (`IDeaktivaceOsobJob`, `DeaktivaceOsobService`) is gone as well — deactivating inactive players is a manual action in the player-management screen.
- **The app does NOT create the database or containers at runtime.** They come from [infra/main.bicep](infra/main.bicep) in Azure and from `MigrationTool` locally — the app assumes they exist. `CosmosDatabaseInitializer` exists in the DataLayer but is never called by the API; creating containers on startup would only lengthen the cold start.
- **There is no data-seed endpoint.** `IDataSeedApi` / `DataSeedFacade` / `CoreProfile` went away with the EF stack — no seed ever existed, so the endpoint was an open, unauthenticated hook to nothing.
- `/api/health` ([Api/Functions/HealthFunctions.cs](src/Api/Functions/HealthFunctions.cs)) registers no checks on purpose — it answers 200 as soon as the app stands. Flex Consumption has no health probes (unlike the previous ACA hosting), so it serves manual checks and cold-start measurement only.
- **There is no rate limiting.** The old `DefaultAPI` limiter (10 req / 5 s) was a blunt guard for a single replica and its database; its role is now played by `maximumInstanceCount: 1` in the bicep template — primarily a cost cap (a flood of the anonymous API bills at most one instance; one 0.25-core instance carries the real traffic with room to spare, and the app has no dependency on instance count) — and by the database's own throughput limit (Cosmos returning 429).
- **There is no OpenAPI document and no Scalar UI.** Azure Functions cannot export one at build time (`Microsoft.Extensions.ApiDescription.Server` is tied to the ASP.NET Core pipeline), and the official `Microsoft.Azure.Functions.Worker.Extensions.OpenApi` is in maintenance mode and broken on .NET 10. Clients don't need it — the contract is the interface.

### MigrationTool (container setup + data transfer both ways)

- Console app ([MigrationTool/Program.cs](src/MigrationTool/Program.cs)): creates the database and containers if they are missing, and optionally moves data between Cosmos and SQL Server. **CI/CD does not touch it**; it is a manual step from a local clone.
- Wiring: `ConfigureForMigrationTool` in [DependencyInjection/ServiceCollectionExtensions.cs](src/DependencyInjection/ServiceCollectionExtensions.cs) (deliberately minimal — DataLayer only, no Services/Facades).
- Parameters: `--endpoint`, `--key` (omit in Azure → `DefaultAzureCredential`), `--database` (no default anywhere — `Cosmos:DatabaseId` must be set explicitly in every environment, the base `appsettings.Api.json` leaves it empty), plus **one** of `--importfromsql <cs>` / `--exporttosql <cs>`. The F5 profile points at the local emulator.
- **[SqlImport.cs](src/MigrationTool/SqlImport.cs)** (`--importfromsql`, SQL → Cosmos) reads the four old tables over raw ADO.NET (no EF — the `Entity` project is gone), collapses přihláška rows into array items, and upserts. **It is re-runnable because the new ids are derived from the old int ids, not random** (`GetId`) — a random GUID per run would duplicate every person on the second pass. Termíny are keyed by date anyway; where SQL held several rows for one date (deleted + active), only the winning row and *its* přihlášky are imported. **The first cutover runs with the Function App stopped**: stop → deploy workflow (bicep creates the containers) → import → start. The import upserts termín documents, so any signup made against an empty database in between would be overwritten (see infra/README.md, *Postup nasazení*).
- **[CosmosExport.cs](src/MigrationTool/CosmosExport.cs)** (`--exporttosql`, Cosmos → SQL) is the way back: it recreates the pre-Cosmos schema (both unique indexes, `__DataSeed`, and `__EFMigrationsHistory` rows so an older build would not try to migrate it again) and pours the documents in. Ids are **not** preserved — they are gone from Cosmos — so identity columns assign new ones and references are remapped in flight. It refuses to run against a database that already has the app's tables, and the whole run (DDL included) is one transaction, so a failure leaves the target empty.
- Both directions are **disposable**, together with the `Microsoft.Data.SqlClient` package, once the move is settled and the old database is retired.
- **Local dev**: run it once after cloning to create `volejbal` in the emulator. `TestsForLocalDebugging` does its own drop+create per test ([TestBase.cs](src/TestsForLocalDebugging/TestBase.cs)).

### Web.Client (Blazor WebAssembly)

- Standalone WASM app on Azure Static Web Apps; SPA fallback is in `wwwroot/staticwebapp.config.json`.
- The API base address comes from `wwwroot/appsettings.json` (`ApiBaseUrl`) — **it is a committed production value** and must match the Function App URL that `infra/main.bicep` outputs. `appsettings.Development.json` points at `http://localhost:7071`.
- Czech locale is set in `Program.cs` (`CultureInfo.DefaultThreadCurrentCulture`); the csproj sets `BlazorWebAssemblyLoadAllGlobalizationData=true` because Czech is not in the default ICU shards — don't remove it or date formatting breaks.
- **No component library and no Bootstrap.** The UI is hand-written markup over one stylesheet, [wwwroot/css/volejbal.css](src/Web.Client/wwwroot/css/volejbal.css) — custom classes, oklch colors, `[data-theme="dark"]` for the dark mode. `Havit.Blazor.Components.Web.Bootstrap` was removed: only five Hx components were ever used, the design system overrode their Bootstrap styling anyway, and the package cost ~330 KB of brotli IL plus ~36 KB of CSS on first load. Forms are the built-in `InputText`/`InputSelect`/`InputTextArea` inside `.field` wrappers, the confirm dialog in [SeznamOsob.razor](src/Web.Client/Components/Pages/Osoby/SeznamOsob.razor) is plain markup driven by Blazor. **The `RESET` block at the top of volejbal.css replaces Bootstrap's Reboot** — the rest of the stylesheet assumes `box-sizing: border-box` and a zeroed `body` margin, so don't drop it. Local state via `Havit.Blazor.Storage` (`ILocalStorageService`).

### Tests

Two test projects, deliberately separated:

- **`Tests/`** — CI tests (run in `dotnet test` step of `build.yml`): the API contract tests described above, the `TerminDatumGeneratorService` date-arithmetic tests, the document-shape tests in [Tests/DataLayer/CosmosSerializationTests.cs](src/Tests/DataLayer/CosmosSerializationTests.cs) (camelCase `id`, omitted nulls, fixed date format, ETag outside the document), and the composition-root test that resolves every facade from the production DI container. Pure MSTest, **no database of any kind** — nothing here needs one any more.
- **`TestsForLocalDebugging/`** — local-only tests against the **Cosmos DB Emulator** (`https://localhost:8081`, config in its `appsettings.json`). Base class [TestsForLocalDebugging/TestBase.cs](src/TestsForLocalDebugging/TestBase.cs) wires up the real DI container via `ConfigureForTests` and drops + recreates the `volejbal-test` database before each test. The one test there proves the 409-retry in `EnsureTerminyService` under real concurrency, which nothing can fake. Do not add these to CI.

## Configuration

- Api: `appsettings.Api.json` + environment override + (Debug only) `*.local.json` (gitignored) + env vars. `local.settings.json` (gitignored) holds the local Functions host settings — Azurite connection and dev CORS. App Insights connection string and `Cosmos__Endpoint` are supplied as app settings from the bicep template in production — no Key Vault.
- **`Cosmos:Key` is empty in every environment except Development.** Empty means "authenticate with `DefaultAzureCredential`" ([CosmosClientFactory](src/DataLayer/Cosmos/CosmosClientFactory.cs)); the Development value is the emulator's well-known public key, not a secret. The deployment therefore carries no credential at all.
- Web.Client: `wwwroot/appsettings.json` (see above).

## Build / coding conventions ([Directory.Build.props](Directory.Build.props) + [.editorconfig](.editorconfig))

- `Nullable` is **disabled** project-wide. Don't sprinkle `?`/`!` expecting NRT semantics.
- `ImplicitUsings` enabled; common usings are pulled in via per-project `GlobalUsings.cs`.
- `DisableTransitiveProjectReferences=true` — if you need a type from a non-direct dependency, add the explicit `ProjectReference`.
- Central package versions in [Directory.Packages.props](Directory.Packages.props) (`ManagePackageVersionsCentrally=true`). Add new packages by `<PackageVersion>` here, then `<PackageReference>` (no version) in the csproj. Application Insights is deliberately held at **2.x** and dependabot ignores `Microsoft.ApplicationInsights*`.
- **`Newtonsoft.Json` is referenced but never used by our code — and it cannot be dropped.** `Microsoft.Azure.Cosmos` no longer *depends* on it (so consumers pin their own version), but three of its four assemblies still *reference* it: `Client.dll` alone binds 40 types (`JsonConvert`, `JsonSerializer`, the whole `Linq.J*` family, `[JsonProperty]`…) and uses them to serialize the SDK's own types — `ContainerProperties`, `ThroughputProperties`, query plans, `Documents.*`. Our `UseSystemTextJsonSerializerWithOptions` covers only the *user payload*, i.e. our documents. Without the package, `new CosmosClient(...)` throws `FileNotFoundException: Newtonsoft.Json, Version=10.0.0.0` **in the constructor** — and since the client is a singleton, that surfaces as a 500 on the first request that touches data, not as a build error. The build target `CheckNewtonsoftJsonPresence` exists to prevent exactly that; `AzureCosmosDisableNewtonsoftJsonCheck=true` only silences it and moves the failure into production. Cost of keeping it: 723 KB in the publish output.
- `Api.csproj` uses the **`Azure.Functions.Sdk` project SDK**, not `Microsoft.NET.Sdk`; its version is pinned in [global.json](global.json) under `msbuild-sdks`. Do not add `OutputType`, `AzureFunctionsVersion` or `FunctionsEnableWorkerIndexing` — the SDK sets them and complains (`AZFW0110`/`AZFW0111`).
- **`AZFW0108`**: a solution-level `dotnet restore` does not run the SDK's restore hook for the generated Functions extensions project, and `func start` invalidates its marker. Both CI workflows therefore run `dotnet restore src/Api/Api.csproj` as a separate step **and then build with `--no-restore`** — a build without that flag does its own solution-level restore, which invalidates the marker again and brings the warning back. Locally: `dotnet restore src/Api/Api.csproj` once, then `dotnet build … --no-restore`.
- **`IsRidAgnostic=false` on every project between `Api` and `DataLayer`** (`DataLayer`, `Services`, `Facades`, `DependencyInjection`). Class libraries are RID-agnostic by default, so the SDK strips the `--runtime linux-x64` of the Api publish before building them. The Cosmos SDK, however, adds its Windows-only native binaries (`ServiceInterop`, `msvcp140`, `vcruntime140*`, ~9 MB) through its own `.targets` file whenever the RID is empty or starts with `win` — so a RID-less build of `DataLayer` produced them, and they flowed transitively into the Linux zip that Flex Consumption downloads on every cold start. With the property set, the publish RID flows down the chain, the condition fails, and the files are never created. **Every project on the path must carry it**: one RID-agnostic project in between builds `DataLayer` again without the RID and the binaries are back. A new project inserted between `Api` and `DataLayer` needs the property too. Nothing changes for RID-less builds (CI build, tests, MigrationTool, F5).
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
- **Azure authentication is OIDC / federated credentials**, no stored password: `permissions: id-token: write` plus `client-id`/`tenant-id`/`subscription-id` on `azure/login`, which come from repository **variables** (`vars.AZURE_*`, not `secrets.*` — they are identifiers, not secrets; using the wrong context yields an empty string and an opaque login failure). The federated credential's subject must be `repo:jirikanda@5111719/VolejbalApp@169379851:environment:Production` — two separate traps in one string: it is an **environment** subject (the job targets `environment: Production`, so a ref-based subject silently fails to match), and the repo has **immutable subject claims** enabled, so the prefix carries owner and repo IDs rather than just names. Check the live prefix with `gh api repos/jirikanda/VolejbalApp/actions/oidc/customization/sub`. **The workflow has no secrets at all** — since the move to Cosmos DB the app authenticates to its database with its managed identity, so the old `DATABASE_CONNECTION_STRING` secret is gone.
- [infra/main.bicep](infra/main.bicep) (see [infra/README.md](infra/README.md) for one-time setup) creates the Log Analytics workspace, Application Insights (workspace-based on it), the storage account + deployment container, the Flex Consumption plan (`FC1`), the Function App, its storage role assignments, **the Cosmos DB account with its database, containers and the data-plane role assignment**, the Static Web App, and **cost watching** — an e-mail action group, a metric alert on `OnDemandFunctionExecutionCount` (the billed meter; > 3000 per 5 min), a **circuit-breaker Logic App** the alert calls through the action group (`POST …/stop`, wait 30 min, `POST …/start`, via its managed identity with *Website Contributor* on the Function App; it acts only on `monitorCondition == 'Fired'`, because `autoMitigate` makes the action group call it again with `Resolved` right after the stop), and a 5 USD monthly budget on the resource group (100 % actual, notify only) — Pay-As-You-Go has no hard spending cap. `Microsoft.Logic` and `Microsoft.Consumption` must be registered in the subscription once (the deploy SP is Owner of the RG only and cannot register providers) — only the DNS record is out of its scope. The template takes **no parameters**.
- **Cosmos free tier is one per subscription and can only be turned on when the account is created.** Flipping `cosmosEnableFreeTier` later does nothing to an existing account.
- **Cold start is the reason this app is on Functions at all.** Measured on the previous ACA hosting with scale-to-zero: **33 s** to first byte (much of it pulling the image from ghcr.io, outside Azure). `PublishReadyToRun` in [Api.csproj](src/Api/Api.csproj) is conditioned on `RuntimeIdentifier != ''` and is the single biggest lever left — passing `-p:PublishReadyToRun=true` on the CLI instead would flow into `Web.Client` (browser-wasm) and fail with NETSDK1095. Similarly, never `dotnet restore` the whole solution with `--runtime`. Smaller levers already pulled: `TieredPGO=false` in Api.csproj (the tier-0 instrumentation never pays back on an instance that lives minutes; tiered compilation itself must stay on, it is what makes R2R code run), the trimmed-down Application Insights modules (see *Api composition*), and the Windows-only Cosmos binaries kept out of the Linux package (`IsRidAgnostic`, see *Build conventions*). Still open: warming the Cosmos client at startup instead of inside the first request. If cold start still hurts, the escape hatch is `alwaysReadyInstanceCount` in the bicep template (~$6.50/month, no free grant) — measure before turning it on.
