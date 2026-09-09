# Nasazení na Azure Functions (Flex Consumption) + Azure Static Web Apps

Bicep šablona ([main.bicep](main.bicep)) pro hosting `Api` (REST API, Azure Functions) a `Web.Client` (Blazor WASM frontend, Static Web App — obsah nasazuje samostatný GitHub Actions job, ne nativní SWA↔GitHub integrace). Vytváří Log Analytics workspace, Application Insights, storage account s deployment containerem, Flex Consumption plán, Function App včetně role assignmentů ke storage, Static Web App a (volitelně, viz níže) binding custom domény na Static Web App. Databáze je mimo scope této šablony — viz komentář v hlavičce `main.bicep`.

| Resource | Typ | Název |
| --- | --- | --- |
| Log Analytics workspace | `Microsoft.OperationalInsights/workspaces` | `jk-volejbal-logs` |
| Application Insights | `Microsoft.Insights/components` | `jk-volejbal-appinsights` |
| Storage account | `Microsoft.Storage/storageAccounts` | `jkvolejbal<uniqueString>` |
| Flex Consumption plán | `Microsoft.Web/serverfarms` | `jk-volejbal-func-plan` |
| Function App (`Api`) | `Microsoft.Web/sites` | `jk-volejbal-func` |
| Static Web App (`Web.Client`, frontend) | `Microsoft.Web/staticSites` | `JkVolejbalSWA` |

Application Insights je **workspace-based** nad Log Analytics workspace. Connection string se do Function App předává referencí (`appInsights.properties.ConnectionString`), takže žádný GitHub secret pro něj není potřeba.

## Proč Functions místo Container Apps

Na předchozím hostingu (Azure Container Apps, `minReplicas: 0`) byl **naměřený studený start 33 sekund**:

```
/health cold : ttfb=33,19 s
/health warm : ttfb=0,08 s
/api/terminy : ttfb=0,38 s (první), 0,08 s (druhé)
```

Podstatnou část tvořil pull image z `ghcr.io` (mimo Azure) a náběh repliky. Flex Consumption nasazuje **zip do blob containeru ve stejném regionu**, takže pull kontejneru z cizího registru odpadá úplně. Cron scale rules, kterými to ACA maskovalo v hracích oknech (pondělí, úterý), tím ztratily smysl a v šabloně nejsou.

### Studený start a `alwaysReadyInstanceCount`

Parametr `alwaysReadyInstanceCount` je **`0`** — aplikace plně škáluje k nule a vejde se do free grantu (100 000 GB-s a 250 000 spuštění měsíčně). Zaplatí se za to studeným startem v jednotkách sekund.

Jedna always-ready instance ho prakticky odstraní, ale stojí **~6,50 USD/měsíc** (baseline 0,000005 USD/GB-s × 0,5 GB × měsíc; na always-ready se free granty **nevztahují**). Zapínejte ji až podle měření, ne preventivně:

```bash
curl -s -o /dev/null -w "ttfb=%{time_starttransfer}s\n" https://jk-volejbal-func.azurewebsites.net/api/health
```

Pro srovnání: `minReplicas: 1` na starém ACA hostingu vycházelo na ~4,20 USD/měsíc.

### Co Functions neumí a jak se to řeší

ASP.NET Core integrace ve Functions dává jen typy (`HttpRequest`, `IActionResult`), **ne middleware pipeline ani routing**. Věci, které dřív obstarával `Web`, mají proto jiná místa:

| Dřív (ASP.NET Core) | Teď |
| --- | --- |
| `UseCors()` + `Cors:AllowedOrigins` | **platformní CORS** v `siteConfig.cors` (tato šablona) |
| `UseErrorToJson()` | [ExceptionHandlingMiddleware](../src/Api/Infrastructure/ExceptionHandlingMiddleware.cs) (worker middleware) |
| `[ApiController]` + `ValidateModelAttribute` | [RequestBodyReader](../src/Api/Infrastructure/RequestBodyReader.cs) (DataAnnotations → 422) |
| `UseRequestLocalization()` | `CultureInfo.DefaultThreadCurrentCulture` v `Program.cs` |
| `AddRateLimiter` (`DefaultAPI`, 10 req/5 s) | **zrušeno**; roli pojistky přebral `maximumInstanceCount` |
| `RecurringJobsBackgroundService` | **Timer trigger** ([RecurringJobsFunctions](../src/Api/Functions/RecurringJobsFunctions.cs)) |
| OpenAPI dokument + Scalar UI | **zrušeno** (klienti ho nepotřebují, viz níže) |
| `TZ=Europe/Prague` | zóna v kódu ([ApplicationTimeService](../src/Services/Infrastructure/TimeService/ApplicationTimeService.cs)) |

> **`TZ` ani `WEBSITE_TIME_ZONE` na Flex Consumption nefungují** — platforma je tiše ignoruje a proces běží v UTC. Do šablony je nepřidávejte; vypadalo by to, že něco nastavují.

Typové klienty pro `Web.Client` generuje **Refit** ze sdílených rozhraní `I*Api` v `Contracts`, ne NSwag z OpenAPI dokumentu. Azure Functions totiž build-time export OpenAPI neumí (`Microsoft.Extensions.ApiDescription.Server` je vázaný na ASP.NET Core pipeline) a oficiální `Microsoft.Azure.Functions.Worker.Extensions.OpenApi` je v maintenance mode a na .NET 10 rozbitý. Detaily jsou v [CLAUDE.md](../CLAUDE.md).

### Přístup ke storage přes managed identity

Function App má system-assigned identitu a šablona jí přiřazuje tři role na storage account: **Storage Blob Data Owner** (deployment balíček + zámky timer triggeru), **Storage Queue Data Contributor** a **Storage Table Data Contributor** (interní metadata hostu). Díky tomu v app settings nejsou žádné klíče — jen `AzureWebJobsStorage__blobServiceUri` a spol.

Role assignmenty vyžadují na resource group oprávnění `Owner` nebo `User Access Administrator`; samotný `Contributor` na ně nestačí.

### Denní strop ingestace

Workspace má `workspaceCapping.dailyQuotaGb` nastavený na **0,25 GB/den** (parametr `logAnalyticsDailyQuotaGb`, `-1` = bez limitu). Protože je App Insights workspace-based, je efektivní strop **minimum ze stropu na App Insights a na workspace**.

> **Není to nástroj běžné optimalizace, je to pojistka.** Při dosažení stropu se sběr **všech** billable dat na zbytek 24hodinového okna **zastaví** — v tu chvíli nevidíte telemetrii, tedy zrovna když se nejspíš něco děje. Dokumentace Azure Monitoru to říká natvrdo: cílem je stropu **nikdy nedosáhnout**, má chytat jen nečekané špičky. Na trvalé snižování objemu slouží [sampling](https://learn.microsoft.com/en-us/azure/azure-monitor/app/sampling) (zapnutý v `host.json`) nebo ingestion-time transformace.
>
> Hodina resetu je pro každý workspace jiná a **nedá se nastavit**. Strop navíc nezastaví sběr přesně na hodnotě — nějaký přesah se očekává a účtuje se.

Rozumné je nastavit si upozornění, že strop padl — jinak se to pozná až podle chybějících dat:

```
_LogOperation | where Category =~ "Ingestion" | where Detail contains "OverQuota"
```

V bicepu je parametr typu `string` a prochází přes `json()`, protože **bicep nemá typ pro desetinná čísla**.

### Custom doména (`volejbal.kanda.eu`)

Doména patří **Static Web App** (frontend), ne Function App — `Api` běží jen na defaultním `*.azurewebsites.net` hostname (viz `functionAppUrl` output). Static Web App validuje vlastnictví domény metodou `cname-delegation`: stačí, aby CNAME už ukazoval na `staticWebApp.properties.defaultHostname`, žádný TXT token navíc netřeba a certifikát si SWA vydá sama automaticky.

Proměnná `bindCustomDomain` v šabloně je zatím `false`, protože při prvním nasazení nové Static Web App na ni ještě DNS neukazuje. Postup (i pro budoucí přesun na jinou/další doménu):

1. Nasadit šablonu (`bindCustomDomain = false`) a přečíst si výstup `staticWebAppDefaultHostname`:
   ```bash
   az deployment group show --resource-group JkVolejbalRG --name jk-volejbal --query properties.outputs
   ```
2. Spustit job `deploy-frontend` (viz níže) a ověřit, že appka na `https://<staticWebAppDefaultHostname>` funguje a volá produkční API.
3. Teprve pak u registrátora domény přepsat `CNAME` `volejbal.kanda.eu` → `staticWebAppDefaultHostname` (jde o ostré přepnutí produkční domény, krátké okno možné nedostupnosti, dokud DNS nepropaguje). Smazat i starý TXT záznam `asuid.volejbal` (byl jen pro dřívější ACA managed certifikát, teď zbytečný).
4. Počkat na propagaci DNS, v [main.bicep](main.bicep) přepnout `bindCustomDomain` na `true`, commitnout a nasadit znovu. Ověřit `az staticwebapp hostname list --name JkVolejbalSWA --resource-group JkVolejbalRG`, že je doména `Ready`.

### Manuální úklid po migraci z Container Apps

ARM v incremental módu **nemaže** resources, které ze šablony zmizely. Po prvním nasazení této šablony proto v resource group zůstanou osiřelé resources z ACA éry a je potřeba je smazat ručně:

```bash
az containerapp delete --name jk-volejbal-ca-web --resource-group JkVolejbalRG --yes
az containerapp env certificate delete --name jk-volejbal-ca-env --resource-group JkVolejbalRG --certificate volejbal-kanda-eu-cert
az containerapp env delete --name jk-volejbal-ca-env --resource-group JkVolejbalRG --yes
```

Dokud běží Container App, platí se za ni — úklid tedy neodkládejte. Package `volejbal-web` v GHCR už nikdo nepoužívá a jde smazat taky.

## Dva workflow

- **`Build`** ([.github/workflows/build.yml](../.github/workflows/build.yml)) — CI pro `master` a PR do masteru: restore → build → test. Nic nepublikuje ani nenasazuje.
- **`Deploy to Azure`** ([.github/workflows/deploy.yml](../.github/workflows/deploy.yml)) — **výhradně ruční** (`workflow_dispatch`), spouští se nad `master`. Dělá celou cestu do produkce, včetně frontendu.

Ruční spuštění deploye je samo o sobě rozhodnutí, co jde do produkce — proto workflow nemá žádný input s verzí: nasazuje se právě odbavený commit.

### Co Deploy dělá

Job **`build`**: restore → build → test → `dotnet publish` projektu `Api` pro `linux-x64` → zip → upload artifactu. Publish je RID-specifický kvůli ReadyToRun (aktivuje se v csproj podle RID) — to je největší jednotlivá páka na studený start.

Job **`deploy`** (`needs: build`): stažení artifactu → Azure login přes OIDC → `az deployment group create` s bicep šablonou → `Azure/functions-action` s `sku: flexconsumption` → výpis URL API.

> **Pořadí uvnitř jobu je závazné**: šablona musí Function App vytvořit a přiřadit její identitě role ke storage **dřív**, než se do ní balíček nahraje.

Job **`deploy-frontend`** (`needs: deploy`): `dotnet publish` na `Web.Client` → Azure login přes OIDC → zjištění deployment tokenu SWA → nahrání statického výstupu přes `Azure/static-web-apps-deploy`. Samostatný job, ne rozšíření `build`/`deploy` — publikace frontendu je nezávislá na publishi API, a selhání jedné části se nemá tvářit jako selhání druhé.

Nahrávání obsahu do SWA samo o sobě OIDC cestu nemá — chce deployment token (API klíč resource). Ten se ale **nikam neukládá**: job si ho přečte za běhu (`az staticwebapp secrets list`) přes tentýž OIDC login jako bicep deploy a zamaskuje ho v logu (`::add-mask::`), takže žije jen v rámci jednoho běhu a není co rotovat.

Rozdělení do jobů není kosmetika: `environment: Production` je na `deploy` i `deploy-frontend` jobu, takže případní required reviewers schvalují až ve chvíli, kdy build i testy prošly, ne naslepo na začátku.

Nasazuje se **celá šablona**, ne jen aplikace. ARM v incremental módu projde nezměněné resources jako no-op, takže to stojí minutu navíc — výměnou za to nemůže infrastruktura začít odpovídat něčemu jinému než šabloně.

## Jednorázová příprava

1. Existující resource group (`az group create -n JkVolejbalRG -l germanywestcentral`) — název musí sedět s `RESOURCE_GROUP` v [deploy.yml](../.github/workflows/deploy.yml). Flex Consumption v `germanywestcentral` dostupný je; ověřit lze `az functionapp list-flexconsumption-locations -o table`.
2. Service principal pro `azure/login` s **federated credentials (OIDC)** — bez hesla, není co rotovat:
   ```bash
   SUB_ID=$(az account show --query id -o tsv)
   TENANT_ID=$(az account show --query tenantId -o tsv)

   # app registration + service principal
   APP_ID=$(az ad app create --display-name volejbal-deploy --query appId -o tsv)
   az ad sp create --id "$APP_ID"

   # federated credential - subject MUSÍ odpovídat tomu, jak job běží (viz poznámka níže)
   az ad app federated-credential create --id "$APP_ID" --parameters '{
     "name": "github-volejbal-production",
     "issuer": "https://token.actions.githubusercontent.com",
     "subject": "repo:jirikanda@5111719/VolejbalApp@169379851:environment:Production",
     "audiences": ["api://AzureADTokenExchange"]
   }'

   # oprávnění na resource group - Owner, ne Contributor: šablona vytváří role assignmenty
   # (přístup Function App ke storage přes managed identity), na což Contributor nestačí
   az role assignment create --assignee "$APP_ID" --role Owner \
     --scope "/subscriptions/$SUB_ID/resourceGroups/JkVolejbalRG"

   echo "AZURE_CLIENT_ID=$APP_ID"; echo "AZURE_TENANT_ID=$TENANT_ID"; echo "AZURE_SUBSCRIPTION_ID=$SUB_ID"
   ```

   > **Owner je tu nutnost, ne pohodlí — a stojí za to vědět proč.** Šablona vytváří tři role assignmenty
   > (Storage Blob Data Owner / Queue Data Contributor / Table Data Contributor) pro managed identity
   > Function App, aby přístup ke storage nepotřeboval klíče. To vyžaduje
   > `Microsoft.Authorization/roleAssignments/write`, a Contributor má `Microsoft.Authorization/*/Write`
   > mezi `notActions` — deploy by skončil na `AuthorizationFailed`.
   >
   > Daň je reálná: automatizace tím dostane právo přidělovat komukoli jakákoli práva, výměnou za
   > odstranění klíče ke storage, na němž leží jen deployment balíček a zámky timer triggeru. Kdyby to
   > mělo někdy vadit, jsou dvě cesty zpět: přepnout `AzureWebJobsStorage` na connection string
   > (role assignmenty ze šablony zmizí a Contributor stačí), nebo Owner zúžit na
   > Contributor + User Access Administrator **s podmínkou**, která omezí přidělitelné role jen na ty tři.
   >
   > **Pokud service principal už existuje s Contributorem** (což je případ tohoto repozitáře — SP byl
   > založený ještě pro Container Apps, které role assignmenty nepotřebovaly), roli je potřeba navýšit
   > zvlášť; `az ad app create` výše se v takovém případě nespouští:
   > ```bash
   > SP_OBJECT_ID=$(az ad sp show --id "$APP_ID" --query id -o tsv)
   > az role assignment create \
   >   --assignee-object-id "$SP_OBJECT_ID" --assignee-principal-type ServicePrincipal \
   >   --role Owner --scope "/subscriptions/$SUB_ID/resourceGroups/JkVolejbalRG"
   > ```
   > Původní Contributor přiřazení pak lze odebrat, nebo nechat — Owner ho pohlcuje.

   > **Pozor na `subject`.** Skládá se ze dvou věcí, které se obě dají snadno splést; když nesedí, login skončí na `AADSTS70021: No matching federated identity record found`.
   >
   > **1. `environment`, ne `ref`.** Job `deploy` má `environment: Production`, a když job cílí na environment, GitHub do tokenu dá claim `…:environment:Production` — **ne** `ref:refs/heads/master`. Kdyby z `deploy.yml` někdy zmizel `environment: Production`, je potřeba credential přenastavit.
   >
   > **2. Immutable subject — jména *i* ID.** Repozitář má zapnuté [immutable subject claims](https://docs.github.com/en/actions/reference/security/oidc#immutable-subject-claims), takže prefix je `repo:jirikanda@5111719/VolejbalApp@169379851`, ne jen `repo:jirikanda/VolejbalApp`. `5111719` je ID účtu, `169379851` ID repozitáře. Díky ID přežije subject přejmenování účtu i repozitáře.
   >
   > Aktuální podobu prefixu ověříte kdykoli:
   > ```bash
   > gh api repos/jirikanda/VolejbalApp/actions/oidc/customization/sub
   > ```

3. `Settings → Secrets and variables → Actions` — pozor, jde o **dvě různé záložky**:

   **Variables:**
   - `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` — z výpisu výše. Jsou to identifikátory, ne tajemství; jako variables jsou navíc čitelné v logu běhu, což ladění usnadňuje. Ve workflow se na ně sahá přes `vars.*`.

   **Secrets:**
   - `DATABASE_CONNECTION_STRING` — connection string k databázi (hostované mimo tuto šablonu). Ve workflow `secrets.*`. **Jediné skutečné tajemství** — Azure login ani nasazení frontendu na SWA žádné uložené heslo nepotřebují.

   > Kdyby se některá z hodnot přesunula mezi záložkami, je potřeba změnit i prefix v [deploy.yml](../.github/workflows/deploy.yml). `secrets.X` u proměnné uložené jako variable se vyhodnotí na **prázdný řetězec** — workflow nespadne na chybějící hodnotu, ale `azure/login` selže na nesrozumitelnou chybu.
4. Environment `Production` v *Settings → Environments* (už existuje). **Velikost písmen musí sedět** — GitHub názvy environmentů nerozlišuje a `environment: production` by se napároval i na `Production`, jenže Entra ID porovnává subject přesně a při neshodě výměnu tokenu odmítne **bez chybové hlášky**. Volitelně sem přidejte *required reviewers*.

Výstup workflow `functionAppUrl` je adresa `https://<app>.azurewebsites.net` API — žádná custom doména se na ni neváže. Výstup `staticWebAppDefaultHostname` je defaultní adresa frontendu, dokud nemá navázanou `volejbal.kanda.eu`.

## Postup nasazení

1. **Zmigrovat databázi**, pokud přibyla migrace — viz níže. Musí být hotové **dřív**, než naběhne nový kód.
2. *Actions* → *Deploy to Azure* → *Run workflow* (branch `master`).

### Ruční nasazení bez GitHub Actions (alternativa)

```bash
az deployment group create \
  --resource-group JkVolejbalRG \
  --template-file deploy/main.bicep \
  --name jk-volejbal \
  --parameters databaseConnectionString='<connection string>'

dotnet publish src/Api/Api.csproj -c Release -r linux-x64 --self-contained false -o ./publish/Api
cd ./publish/Api && zip -r ../../api.zip . && cd ../..
az functionapp deployment source config-zip \
  --resource-group JkVolejbalRG --name jk-volejbal-func --src api.zip
```

Tohle je zároveň cesta k **rollbacku** — workflow vždy nasazuje aktuální commit, takže návrat na starší verzi znamená buď tenhle postup ze staršího checkoutu, nebo revert v `master` a nový běh workflow.

### Ověření šablony před nasazením

```bash
az bicep build --file deploy/main.bicep --stdout > /dev/null   # jen syntaxe, nepotřebuje login
az deployment group what-if --resource-group JkVolejbalRG --name jk-volejbal \
  --template-file deploy/main.bicep --parameters databaseConnectionString="Server=x;Database=y;User Id=z;Password=q"
```

What-if se nepřipojí k databázi, takže fiktivní hodnota parametru stačí. Dva warningy `BCP081` u `Microsoft.Web/staticSites` jsou očekávané — bicep pro tuhle verzi API nemá typy, na nasazení to vliv nemá.

## Poznámky

- **CORS a `ApiBaseUrl` jdou ruku v ruce.** `Api` a `Web.Client` běží na různých originech (Function App vs. SWA), takže Function App potřebuje CORS allow-list (nastavený v `main.bicep` na hostname Static Web App i na custom doménu) a `Web.Client` potřebuje vědět, kam volat — to je commitnutá hodnota `ApiBaseUrl` v `src/Web.Client/wwwroot/appsettings.json`. **Obě hodnoty jsou napsané ručně a nic je nepropojuje automaticky**; při změně názvu Function App je nutné upravit obojí.
- **HTTPS řeší platforma** (`httpsOnly: true`), ne aplikace. Ve Functions není `UseHttpsRedirection` ani `UseHsts` — a není kam je dát, ASP.NET Core middleware pipeline tu neexistuje.
- **`/api/health` nemá registrované žádné checky** — vrací 200, jakmile stojí aplikace. Flex Consumption health probes nemá (na rozdíl od ACA), takže endpoint slouží ručnímu ověření a měření studeného startu, ne platformě.
- **Aplikace nemá vazbu na počet instancí.** Plánovač už není in-process (Timer trigger místo `RecurringJobsBackgroundService`), takže dřívější závazné `maxReplicas: 1` neplatí. `maximumInstanceCount: 5` je pojistka proti přetížení sdílené databáze, ne funkční nutnost.
- **App init timeout je 30 s.** Když se aplikace nerozběhne dřív, host to logne jako gRPC `System.TimeoutException` a hodnota se nedá konfigurovat. Sledovat při přidávání práce do startu.
- Migrace databázového schématu a data seedy řeší samostatný konzolový **`MigrationTool`** projekt (mimo scope téhle šablony). Workflow ho **záměrně nepublikuje ani nespouští** — aplikace schéma za běhu nemigruje, takže ho musíte pustit sám z lokálního repa proti produkčnímu connection stringu, **před** spuštěním deploye:
  ```powershell
  dotnet run --project src/MigrationTool -- --connectionstring "<connection string>"
  ```
  Automatizace tohohle kroku by znamenala vyřešit, jak se runner dostane k databázi po síti — u Azure SQL firewall pravidlo pro dynamickou IP hosted runneru, což je nepříjemné.
