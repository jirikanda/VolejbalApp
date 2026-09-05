# Nasazení na Azure Container Apps + Azure Static Web Apps

Bicep šablona ([main.bicep](main.bicep)) pro hosting `Web` (REST API, Container App) a `Web.Client` (Blazor WASM frontend, Static Web App — obsah nasazuje samostatný GitHub Actions job, ne nativní SWA↔GitHub integrace). Vytváří Log Analytics workspace, Application Insights, Container Apps Environment, Container App, Static Web App a (volitelně, viz níže) binding custom domény na Static Web App. Databáze je mimo scope této šablony — viz komentář v hlavičce `main.bicep`.

| Resource | Typ | Název |
| --- | --- | --- |
| Log Analytics workspace | `Microsoft.OperationalInsights/workspaces` | `jk-volejbal-logs` |
| Application Insights | `Microsoft.Insights/components` | `jk-volejbal-appinsights` |
| Container Apps Environment | `Microsoft.App/managedEnvironments` | `jk-volejbal-ca-env` |
| Container App (`Web`, API) | `Microsoft.App/containerApps` | `jk-volejbal-ca-web` |
| Static Web App (`Web.Client`, frontend) | `Microsoft.Web/staticSites` | `JkVolejbalSWA` |

Application Insights je **workspace-based** nad tímtéž Log Analytics workspace, který používá Container Apps Environment pro logy kontejneru — telemetrie aplikace i logy tak končí na jednom místě. Connection string se do Container App předává referencí (`appInsights.properties.ConnectionString`), takže žádný GitHub secret pro něj není potřeba.

### Denní strop ingestace

Workspace má `workspaceCapping.dailyQuotaGb` nastavený na **0,25 GB/den** (parametr `logAnalyticsDailyQuotaGb`, `-1` = bez limitu). Protože je App Insights workspace-based, je efektivní strop **minimum ze stropu na App Insights a na workspace** — na workspace tedy pokrývá jak telemetrii aplikace, tak konzoli kontejneru jedním nastavením.

> **Není to nástroj běžné optimalizace, je to pojistka.** Při dosažení stropu se sběr **všech** billable dat na zbytek 24hodinového okna **zastaví** — v tu chvíli nevidíte ani telemetrii, ani logy kontejneru, tedy zrovna když se nejspíš něco děje. Dokumentace Azure Monitoru to říká natvrdo: cílem je stropu **nikdy nedosáhnout**, má chytat jen nečekané špičky. Na trvalé snižování objemu slouží [sampling](https://learn.microsoft.com/en-us/azure/azure-monitor/app/sampling) nebo ingestion-time transformace.
>
> Hodina resetu je pro každý workspace jiná a **nedá se nastavit**. Strop navíc nezastaví sběr přesně na hodnotě — nějaký přesah se očekává a účtuje se.

Rozumné je nastavit si upozornění, že strop padl — jinak se to pozná až podle chybějících dat:

```
_LogOperation | where Category =~ "Ingestion" | where Detail contains "OverQuota"
```

V bicepu je parametr typu `string` a prochází přes `json()`, protože **bicep nemá typ pro desetinná čísla** — stejný důvod, proč je v šabloně `cpu: json('0.25')`.

### Custom doména (`volejbal.kanda.eu`)

Doména patří **Static Web App** (frontend), ne Container App — `Web` (API) běží jen na defaultním `*.azurecontainerapps.io` hostname (viz `containerAppUrl` output). Static Web App validuje vlastnictví domény metodou `cname-delegation`: stačí, aby CNAME už ukazoval na `staticWebApp.properties.defaultHostname`, žádný TXT token navíc netřeba (na rozdíl od dřívějšího ACA postupu) a certifikát si SWA vydá sama automaticky.

Proměnná `bindCustomDomain` v šabloně je zatím `false`, protože při prvním nasazení nové Static Web App na ni ještě DNS neukazuje. Postup (i pro budoucí přesun na jinou/další doménu):

1. Nasadit šablonu (`bindCustomDomain = false`) a přečíst si výstup `staticWebAppDefaultHostname`:
   ```bash
   az deployment group show --resource-group JkVolejbalRG --name jk-volejbal --query properties.outputs
   ```
2. Spustit job `deploy-frontend` (viz níže) a ověřit, že appka na `https://<staticWebAppDefaultHostname>` funguje a volá produkční API.
3. Teprve pak u registrátora domény přepsat `CNAME` `volejbal.kanda.eu` → `staticWebAppDefaultHostname` (dřív mířil na FQDN Container App — jde o ostré přepnutí produkční domény, krátké okno možné nedostupnosti/neplatnosti, dokud DNS nepropaguje). Smazat i starý TXT záznam `asuid.volejbal` (byl jen pro dřívější ACA managed certifikát, teď zbytečný).
4. Počkat na propagaci DNS, v [main.bicep](main.bicep) přepnout `bindCustomDomain` na `true`, commitnout a nasadit znovu. Ověřit `az staticwebapp hostname list --name JkVolejbalSWA --resource-group JkVolejbalRG`, že je doména `Ready`.

**Manuální úklid po prvním nasazení této šablony** (odstranění custom domény z Container App vyrobilo v Azure osiřelý resource, protože ARM v incremental módu nemaže resources, které zmizely ze šablony): smazat starý `managedCertificate` (`az containerapp env certificate delete --name jk-volejbal-ca-env --resource-group JkVolejbalRG --certificate volejbal-kanda-eu-cert`), pokud tam ještě je.

## Dva workflow

- **`Build`** ([.github/workflows/build.yml](../.github/workflows/build.yml)) — CI pro `master` a PR do masteru: restore → build → test. Nic nepublikuje ani nenasazuje.
- **`Deploy to Azure Container Apps`** ([.github/workflows/deploy.yml](../.github/workflows/deploy.yml)) — **výhradně ruční** (`workflow_dispatch`), spouští se nad `master`. Dělá celou cestu do produkce, včetně frontendu.

Ruční spuštění deploye je samo o sobě rozhodnutí, co jde do produkce — proto workflow nemá žádný `imageTag` input: staví se právě odbavený commit a image se taguje jeho SHA.

### Co Deploy dělá

Job **`build`**: restore → build → test → build a push container image do `ghcr.io/<owner>/volejbal-web:<commit-sha>`.

Image vzniká přes vestavěnou kontejnerizaci .NET SDK (`dotnet publish -t:PublishContainer`, žádný Dockerfile) — obsahuje jen `Web` (API), `Web.Client` (frontend) se nasazuje samostatně, viz níže. Push autentizuje automatický `GITHUB_TOKEN` (`packages: write`), žádný extra secret netřeba.

Job **`deploy`** (`needs: build`): Azure login přes OIDC → `az deployment group create` s bicep šablonou (vytváří/aktualizuje i Static Web App resource, ne jen Container App) → výpis URL API.

Job **`deploy-frontend`** (`needs: deploy`): `dotnet publish` na `Web.Client` → Azure login přes OIDC → zjištění deployment tokenu SWA → nahrání statického výstupu přes `Azure/static-web-apps-deploy`. Samostatný job, ne rozšíření `build`/`deploy` — publikace frontendu je nezávislá na kontejnerovém publish API, a stejně jako u rozdělení `build`/`deploy` nemá selhání jedné části vypadat jako selhání druhé.

Nahrávání obsahu do SWA samo o sobě OIDC cestu nemá — chce deployment token (API klíč resource). Ten se ale **nikam neukládá**: job si ho přečte za běhu (`az staticwebapp secrets list`) přes tentýž OIDC login jako bicep deploy a zamaskuje ho v logu (`::add-mask::`), takže žije jen v rámci jednoho běhu a není co rotovat. Federated credential se kvůli tomu měnit nemusí — subject je vázaný na `environment: Production`, který má i tenhle job.

Rozdělení do jobů není kosmetika: `environment: Production` je na `deploy` i `deploy-frontend` jobu, takže případní required reviewers schvalují až ve chvíli, kdy build i testy prošly, ne naslepo na začátku.

Nasazuje se **celá šablona**, ne jen výměna image. ARM v incremental módu projde nezměněné resources jako no-op, takže to stojí minutu navíc — výměnou za to nemůže infrastruktura začít odpovídat něčemu jinému než šabloně. Pozor: incremental mode taky **nemaže** resources, které ze šablony zmizely (viz poznámka o manuálním úklidu `managedCertificate` výše).

### Jak se Container App dozví o novém image

Sama, není potřeba jí nic říkat. Změna `containerImage` je změna v `template` Container App, a jakákoli změna template vytvoří **novou revizi**; při `activeRevisionsMode: 'Single'` na ni ACA překlopí veškerý provoz, jakmile naběhne, a starou deaktivuje.

Pozor na rozdíl proti secretům: ty jsou vlastnost aplikace, ne revize, takže změna hodnoty secretu sama o sobě novou revizi **nevyrobí** ani běžící kontejner nerestartuje. Při běžném deployi to nevadí (mění se i image tag), ale kdybyste měnil jen connection string, musíte si revizi vynutit.

### Package v GHCR

Package `volejbal-web` je nastavený jako **veřejný**, takže ho Container App pullne anonymně — `main.bicep` proto nemá `registries` blok ani žádný secret s PAT. GHCR dává novým packages defaultně privátní viditelnost; kdyby se package někdy vrátil na privátní (nebo vznikl nový), pull by začal padat na `UNAUTHORIZED` a bylo by potřeba do šablony doplnit `registries` + secret s classic PAT se scope `read:packages`.

## Jednorázová příprava

1. Existující resource group (`az group create -n JkVolejbalRG -l germanywestcentral`) — název musí sedět s `RESOURCE_GROUP` v [deploy.yml](../.github/workflows/deploy.yml).
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

   # oprávnění na resource group
   az role assignment create --assignee "$APP_ID" --role Contributor \
     --scope "/subscriptions/$SUB_ID/resourceGroups/JkVolejbalRG"

   echo "AZURE_CLIENT_ID=$APP_ID"; echo "AZURE_TENANT_ID=$TENANT_ID"; echo "AZURE_SUBSCRIPTION_ID=$SUB_ID"
   ```

   > **Pozor na `subject`.** Skládá se ze dvou věcí, které se obě dají snadno splést; když nesedí, login skončí na `AADSTS70021: No matching federated identity record found`.
   >
   > **1. `environment`, ne `ref`.** Job `deploy` má `environment: Production`, a když job cílí na environment, GitHub do tokenu dá claim `…:environment:Production` — **ne** `ref:refs/heads/master`. Kdyby z `deploy.yml` někdy zmizel `environment: Production`, je potřeba credential přenastavit.
   >
   > **2. Immutable subject — jména *i* ID.** Repozitář má zapnuté [immutable subject claims](https://docs.github.com/en/actions/reference/security/oidc#immutable-subject-claims), takže prefix je `repo:jirikanda@5111719/VolejbalApp@169379851`, ne jen `repo:jirikanda/VolejbalApp`. `5111719` je ID účtu, `169379851` ID repozitáře. Díky ID přežije subject přejmenování účtu i repozitáře — u jmenné varianty by po přejmenování mohl trust policy nečekaně splnit někdo jiný, kdo si uvolněné jméno zabere.
   >
   > Aktuální podobu prefixu ověříte kdykoli:
   > ```bash
   > gh api repos/jirikanda/VolejbalApp/actions/oidc/customization/sub
   > ```

3. `Settings → Secrets and variables → Actions` — pozor, jde o **dvě různé záložky**:

   **Variables:**
   - `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` — z výpisu výše. Jsou to identifikátory, ne tajemství; jako variables jsou navíc čitelné v logu běhu, což ladění usnadňuje. Ve workflow se na ně sahá přes `vars.*`.

   **Secrets:**
   - `DATABASE_CONNECTION_STRING` — connection string k databázi (hostované mimo tuto šablonu). Ve workflow `secrets.*`. **Jediné skutečné tajemství** — GHCR pull, Azure login ani nasazení frontendu na SWA žádné uložené heslo nepotřebují (viz níže).

   > Kdyby se některá z hodnot přesunula mezi záložkami, je potřeba změnit i prefix v [deploy.yml](../.github/workflows/deploy.yml). `secrets.X` u proměnné uložené jako variable se vyhodnotí na **prázdný řetězec** — workflow nespadne na chybějící hodnotu, ale `azure/login` selže na nesrozumitelnou chybu.
4. Environment `Production` v *Settings → Environments* (už existuje). **Velikost písmen musí sedět** — GitHub názvy environmentů nerozlišuje a `environment: production` by se napároval i na `Production`, jenže Entra ID porovnává subject přesně a při neshodě výměnu tokenu odmítne **bez chybové hlášky**. Proto je `Production` s velkým P jak v [deploy.yml](../.github/workflows/deploy.yml), tak v subjectu credentialu. Volitelně sem přidejte *required reviewers* — pak se `deploy`/`deploy-frontend` job zastaví a počká na schválení.

Výstup workflow `containerAppUrl` je adresa `https://<app>.<region>.azurecontainerapps.io` API — žádná custom doména se na ni neváže. Výstup `staticWebAppDefaultHostname` je defaultní adresa frontendu, dokud nemá navázanou `volejbal.kanda.eu` (viz sekce o custom doméně výše).

## Postup nasazení

1. **Zmigrovat databázi**, pokud přibyla migrace — viz níže. Musí být hotové **dřív**, než naběhne nový kód.
2. *Actions* → *Deploy to Azure Container Apps* → *Run workflow* (branch `master`).

### Ruční nasazení bez GitHub Actions (alternativa)

Předpokládá, že image s daným SHA už v GHCR je:

```bash
az deployment group create \
  --resource-group JkVolejbalRG \
  --template-file deploy/main.bicep \
  --name jk-volejbal \
  --parameters \
    containerImage='ghcr.io/<github-user>/volejbal-web:<commit-sha>' \
    databaseConnectionString='<connection string>'
```

Tohle je zároveň cesta k **rollbacku** — workflow vždy nasazuje aktuální commit, takže návrat na starší verzi znamená buď tenhle příkaz se starším SHA, nebo revert v `master` a nový běh workflow.

### Ověření šablony před nasazením

```bash
az bicep build --file deploy/main.bicep --stdout > /dev/null   # jen syntaxe, nepotřebuje login
az deployment group what-if --resource-group JkVolejbalRG --name jk-volejbal --template-file deploy/main.bicep --parameters containerImage="ghcr.io/jirikanda/volejbal-web:dummy" databaseConnectionString="Server=x;Database=y;User Id=z;Password=q"
```

What-if nepullne image ani se nepřipojí k databázi, takže fiktivní hodnoty parametrů stačí.

## Poznámky

- **Přesměrování HTTP → HTTPS dělá ingress** (`allowInsecure: false`), ne aplikace. `UseHttpsRedirection` v [Web/Startup.cs](../src/Web/Startup.cs) je v kontejneru **fakticky no-op** — middleware potřebuje znát cílový HTTPS port, Kestrel poslouchá jen na HTTP a žádný `https_port` nastavený není, takže jen zaloguje `Failed to determine the https port for redirect`. Zůstává kvůli lokálnímu běhu a pro případ hostování mimo ACA. Ochranu prohlížeče drží `UseHsts()`.

  Je **podmíněný přes `UseWhen`, aby minul `/health`**. Dnes je to zbytečné (middleware stejně nepřesměrovává), ale je to pojistka: kdyby kdykoli ožil, odbavoval by probes 307 redirectem — a ACA bere jako úspěch cokoli v rozsahu 200–399, takže by probe procházel i u úplně rozbité aplikace. Takové selhání je tiché.
- **`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`** je nutný — ACA ingress terminuje TLS a do kontejneru posílá HTTP. Bez zpracování `X-Forwarded-*` by aplikace každý request viděla jako nešifrovaný, což má dva konkrétní dopady: `UseHsts()` hlavičku na non-HTTPS requesty nepřidává, takže by HSTS tiše nikdy nefungoval, a do telemetrie by se místo IP klienta zapisovala interní adresa ingressu.
- **`maxReplicas: 1` je závazné, ne jen výchozí hodnota** — [Services/Jobs/RecurringJobsBackgroundService.cs](../src/Services/Jobs/RecurringJobsBackgroundService.cs) je in-process plánovač bez distribuovaného zámku; dvě repliky by znamenaly, že se joby (EnsureTerminy, DeaktivaceOsob) spouští duplicitně.
- **Health probes jsou definované explicitně**, protože výchozí readiness probe má `initialDelaySeconds: 3` a `periodSeconds: 5` — a v `Single` revision mode se provoz překlápí až po jeho úspěchu, takže dávno připravená replika čeká na routování několik sekund navíc. To je přímá daň na studeném startu. Rozdělení rolí: **startup** probe se ptá po sekundě (jen on rozhoduje o délce studeného startu — dokud neuspěje, readiness ani liveness neběží), **readiness** po deseti a **liveness** po třiceti (na cold start už nemají vliv, takže není důvod platit za častější dotazy).

  Endpoint je `/health` ([HealthCheckEndpoints.Path](../src/Web/Infrastructure/HealthChecks/HealthCheckEndpoints.cs)) a **nemá registrované žádné checky** — vrací 200, jakmile stojí pipeline. Kontrola databáze by prodloužila studený start a při `maxReplicas: 1` by z výpadku databáze udělala výpadek celé aplikace.

  Telemetrie health requestů se zahazuje ([IgnoreHealthChecksTelemetryProcessor](../src/Web/Infrastructure/ApplicationInsights/IgnoreHealthChecksTelemetryProcessor.cs)). Readiness + liveness dělají přes 11 000 requestů denně; bez filtru by ukusovaly z denního stropu ingestace.
- **`minReplicas: 0`** (scale-to-zero) znamená, že po periodě nečinnosti aplikace "usne" a další request ji probudí (cold start). Startovní běh `EnsureTerminy` v `RecurringJobsBackgroundService` přitom zahřeje EF Core model i connection pool, takže sestavování modelu už první request neplatí.

  **Zbytek studené cesty ale zahřátý není** a je to vědomé rozhodnutí, ne opomenutí. `EnsureTerminy` jde přímo přes službu, nikdy ne přes Kestrel — takže první reálný request stále platí routing, aktivaci controlleru, model binding, metadata System.Text.Json pro DTO a hlavně compiled-query cache EF Core pro *konkrétní tvary* dotazů (`EnsureTerminy` má úplně jiný tvar než `GetTerminyAsync`). Dřív to řešila `WarmupBackgroundService`, která po `ApplicationStarted` střílela loopback HTTP requesty na `/api/nastenka`, `/api/osoby/aktivni` a `/api/terminy`; **naměřeno tehdy: bez ní ~100 ms na první request, s ní ~5–40 ms** (tedy jako zahřátý). Byla odstraněna jako složitost navíc — u téhle aplikace se ~100 ms na probuzení unese. Kdyby se cold start někdy stal problémem, je tohle první místo, kam sáhnout (viz `git log -- src/Web/Infrastructure/WarmupBackgroundService.cs`).
- **CORS a `ApiBaseUrl` jdou ruku v ruce.** `Web` (API) a `Web.Client` (frontend) běží na různých originech (SWA vs. ACA), takže `Web` potřebuje `Cors:AllowedOrigins` (nastavené v `main.bicep` na hostname Static Web App i na custom doménu, viz výše) a `Web.Client` potřebuje vědět, kam volat — to je commitnutá hodnota `ApiBaseUrl` v `src/Web.Client/wwwroot/appsettings.json` (produkční `containerAppUrl` hostname). Když se `Web` (API) přesune jinam (např. na Azure Functions), obě hodnoty je potřeba aktualizovat ručně — nic je nepropojuje automaticky.
- Migrace databázového schématu a data seedy řeší samostatný konzolový **`MigrationTool`** projekt (mimo scope téhle šablony). Workflow ho **záměrně nepublikuje ani nespouští** — aplikace schéma za běhu nemigruje, takže ho musíte pustit sám z lokálního repa proti produkčnímu connection stringu, **před** spuštěním deploye:
  ```powershell
  dotnet run --project src/MigrationTool -- --connectionstring "<connection string>"
  ```
  Automatizace tohohle kroku by znamenala vyřešit, jak se runner dostane k databázi po síti — u Azure SQL firewall pravidlo pro dynamickou IP hosted runneru, což je nepříjemné.
