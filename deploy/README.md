# Nasazení na Azure Container Apps

Bicep šablona ([main.bicep](main.bicep)) pro hosting `Web` (Blazor WASM host + REST API v jedné aplikaci). Vytváří Log Analytics workspace, Application Insights, Container Apps Environment a Container App. Databáze a custom doména jsou mimo scope této šablony — viz komentář v hlavičce `main.bicep`.

| Resource | Typ | Název |
| --- | --- | --- |
| Log Analytics workspace | `Microsoft.OperationalInsights/workspaces` | `jk-volejbal-logs` |
| Application Insights | `Microsoft.Insights/components` | `jk-volejbal-appinsights` |
| Container Apps Environment | `Microsoft.App/managedEnvironments` | `jk-volejbal-ca-env` |
| Container App | `Microsoft.App/containerApps` | `jk-volejbal-ca-web` |

Application Insights je **workspace-based** nad tímtéž Log Analytics workspace, který používá Container Apps Environment pro logy kontejneru — telemetrie aplikace i logy tak končí na jednom místě. Connection string se do Container App předává referencí (`appInsights.properties.ConnectionString`), takže žádný GitHub secret pro něj není potřeba.

## Dva workflow

- **`Build`** ([.github/workflows/build.yml](../.github/workflows/build.yml)) — CI pro `master` a PR do masteru: restore → build → test. Nic nepublikuje ani nenasazuje.
- **`Deploy to Azure Container Apps`** ([.github/workflows/deploy.yml](../.github/workflows/deploy.yml)) — **výhradně ruční** (`workflow_dispatch`), spouští se nad `master`. Dělá celou cestu do produkce.

Ruční spuštění deploye je samo o sobě rozhodnutí, co jde do produkce — proto workflow nemá žádný `imageTag` input: staví se právě odbavený commit a image se taguje jeho SHA.

### Co Deploy dělá

Job **`build`**: restore → build → test → publish `MigrationTool` jako artefakt → build a push container image do `ghcr.io/<owner>/volejbal-web:<commit-sha>`.

Image vzniká přes vestavěnou kontejnerizaci .NET SDK (`dotnet publish -t:PublishContainer`, žádný Dockerfile); WASM klient je v něm zabalený v `wwwroot/_framework`. Push autentizuje automatický `GITHUB_TOKEN` (`packages: write`), žádný extra secret netřeba.

Job **`deploy`** (`needs: build`): Azure login přes OIDC → `az deployment group create` s bicep šablonou → výpis URL.

Rozdělení na dva joby není kosmetika: `environment: Production` je na `deploy` jobu, takže případní required reviewers schvalují až ve chvíli, kdy build i testy prošly, ne naslepo na začátku.

Nasazuje se **celá šablona**, ne jen výměna image. ARM v incremental módu projde nezměněné resources jako no-op, takže to stojí minutu navíc — výměnou za to nemůže infrastruktura začít odpovídat něčemu jinému než šabloně.

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
   - `DATABASE_CONNECTION_STRING` — connection string k databázi (hostované mimo tuto šablonu). Jediné skutečné tajemství — GHCR pull i Azure login žádné heslo nepotřebují. Ve workflow `secrets.*`.

   > Kdyby se některá z hodnot přesunula mezi záložkami, je potřeba změnit i prefix v [deploy.yml](../.github/workflows/deploy.yml). `secrets.X` u proměnné uložené jako variable se vyhodnotí na **prázdný řetězec** — workflow nespadne na chybějící hodnotu, ale `azure/login` selže na nesrozumitelnou chybu.
4. Environment `Production` v *Settings → Environments* (už existuje). **Velikost písmen musí sedět** — GitHub názvy environmentů nerozlišuje a `environment: production` by se napároval i na `Production`, jenže Entra ID porovnává subject přesně a při neshodě výměnu tokenu odmítne **bez chybové hlášky**. Proto je `Production` s velkým P jak v [deploy.yml](../.github/workflows/deploy.yml), tak v subjectu credentialu. Volitelně sem přidejte *required reviewers* — pak se `deploy` job zastaví a počká na schválení.

Výstup workflow (`containerAppUrl`) je veřejná adresa (`https://<app>.<region>.azurecontainerapps.io`), dokud není navázaná custom doména.

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

- **`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`** je nutný — ACA ingress terminuje TLS a do kontejneru posílá HTTP; bez tohohle by `app.UseHttpsRedirection()` (viz [Web/Startup.cs](../Web/Startup.cs)) způsobil nekonečnou smyčku redirectů, protože by appka nerozpoznala, že originální request byl HTTPS.
- **`maxReplicas: 1` je závazné, ne jen výchozí hodnota** — [Services/Jobs/RecurringJobsBackgroundService.cs](../Services/Jobs/RecurringJobsBackgroundService.cs) je in-process plánovač bez distribuovaného zámku; dvě repliky by znamenaly, že se joby (EnsureTerminy, DeaktivaceOsob) spouští duplicitně.
- **`minReplicas: 0`** (scale-to-zero) znamená, že po period nečinnosti aplikace "usne" a další request ji probudí (cold start). [Web/Infrastructure/WarmupBackgroundService.cs](../Web/Infrastructure/WarmupBackgroundService.cs) po startu sám zavolá klíčové endpointy, aby EF Core/MVC pipeline byly zahřáté ještě před prvním reálným requestem.
- Migrace databázového schématu a data seedy řeší samostatný konzolový **`MigrationTool`** projekt (mimo scope téhle šablony). Workflow ho publikuje jako artefakt `MigrationTool` (linux-x64, framework-dependent), ale **záměrně ho nespouští** — aplikace schéma za běhu nemigruje, takže artefakt musíte stáhnout a spustit sám proti produkčnímu connection stringu, **před** deployem nové verze:
  ```bash
  ./KandaEu.Volejbal.MigrationTool --connectionstring "<connection string>"
  ```
  Artefakt z posledního běhu najdete v souhrnu workflow. Automatizace tohohle kroku by znamenala vyřešit, jak se runner dostane k databázi po síti.
