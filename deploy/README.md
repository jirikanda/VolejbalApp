# Nasazení na Azure Container Apps

Bicep šablona ([main.bicep](main.bicep)) pro hosting `Web` (Blazor WASM host + REST API v jedné aplikaci). Databáze, Application Insights a custom doména jsou mimo scope této šablony — viz komentář v hlavičce `main.bicep`.

## Container image — CI ano, CD ne

`.github/workflows/build.yml` na push do `master` (nebo ruční `workflow_dispatch`, ale ne na PR) zabuildí `Web` přes vestavěnou kontejnerizaci .NET SDK (`dotnet publish -t:PublishContainer`, žádný Dockerfile) a pushne na `ghcr.io/<github-user>/volejbal-web`. Tag je jen commit SHA — **záměrně žádný `latest`**: tenhle krok jen publikuje image, nic nedeployuje, a floating tag by byl implicitní deploy target bez záměrného rozhodnutí, co jde do produkce. Autentizace jde přes automatický `GITHUB_TOKEN` (`packages: write`), žádný extra secret v CI netřeba.

Image je v GHCR defaultně **privátní** — buď ho v nastavení package na GitHubu zveřejněte (pak `ghcrPat` secret v deploy workflow nastavit nemusíte), nebo vytvořte GitHub classic PAT se scope `read:packages`, který Container App použije k pull.

Skutečné nasazení (CD) je oddělený **ruční** workflow — viz níže.

## Nasazení — ruční workflow (`.github/workflows/deploy.yml`)

Standardní cesta: záložka *Actions* → *Deploy to Azure Container Apps* → *Run workflow*, zadat commit SHA image, který se má nasadit (najdete ho v běhu workflow *Build*, který image pushnul).

### Jednorázová příprava

1. Existující resource group (`az group create -n volejbal -l westeurope`).
2. Service principal pro `azure/login` a jeho uložení jako GitHub secret `AZURE_CREDENTIALS`:
   ```bash
   az ad sp create-for-rbac --name volejbal-deploy --role Contributor \
     --scopes /subscriptions/$(az account show --query id -o tsv)/resourceGroups/volejbal \
     --json-auth
   ```
   Celý JSON výstup → repo *Settings → Secrets and variables → Actions → New repository secret* → `AZURE_CREDENTIALS`.
3. Další secrets (`Settings → Secrets and variables → Actions`, případně jako environment secrets pod `production`, viz níže):
   - `GHCR_PAT` — GitHub classic PAT se scope `read:packages` (nechte secret prázdný/nevytvářejte, pokud je package veřejný).
   - `DATABASE_CONNECTION_STRING` — connection string k databázi (hostované mimo tuto šablonu).
   - `APPLICATIONINSIGHTS_CONNECTION_STRING` — connection string k existujícímu Application Insights.
4. Volitelně: v *Settings → Environments* vytvořit environment `production` a přidat *required reviewers* — pak `deploy.yml` (má `environment: production`) počká na schválení, než doopravdy nasadí.

Výstup workflow (`containerAppUrl`) je veřejná adresa (`https://<app>.<region>.azurecontainerapps.io`), dokud není navázaná custom doména.

### Ruční nasazení bez GitHub Actions (alternativa)

```bash
az deployment group create \
  --resource-group volejbal \
  --template-file deploy/main.bicep \
  --parameters \
    containerImage='ghcr.io/<github-user>/volejbal-web:<commit-sha>' \
    ghcrUsername='<github-user>' \
    ghcrPat='<pat>' \
    databaseConnectionString='<connection string>' \
    applicationInsightsConnectionString='<connection string>'
```

### Aktualizace image (redeploy)

Opakované spuštění workflow (nebo `az deployment group create` výše) s jiným `imageTag`/`containerImage` vytvoří novou revizi Container App a přepne na ni provoz. Bez potřeby měnit secrets, pokud se connection stringy nezměnily.

## Poznámky

- **`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`** je nutný — ACA ingress terminuje TLS a do kontejneru posílá HTTP; bez tohohle by `app.UseHttpsRedirection()` (viz [Web/Startup.cs](../Web/Startup.cs)) způsobil nekonečnou smyčku redirectů, protože by appka nerozpoznala, že originální request byl HTTPS.
- **`maxReplicas: 1` je závazné, ne jen výchozí hodnota** — [Services/Jobs/RecurringJobsBackgroundService.cs](../Services/Jobs/RecurringJobsBackgroundService.cs) je in-process plánovač bez distribuovaného zámku; dvě repliky by znamenaly, že se joby (EnsureTerminy, DeaktivaceOsob) spouští duplicitně.
- **`minReplicas: 0`** (scale-to-zero) znamená, že po period nečinnosti aplikace "usne" a další request ji probudí (cold start). [Web/Infrastructure/WarmupBackgroundService.cs](../Web/Infrastructure/WarmupBackgroundService.cs) po startu sám zavolá klíčové endpointy, aby EF Core/MVC pipeline byly zahřáté ještě před prvním reálným requestem.
- Migrace databázového schématu a data seedy řeší samostatný konzolový **`MigrationTool`** projekt (mimo scope téhle šablony) — spouští se ručně nebo z vlastní pipeline proti connection stringu, který je zde předán aplikaci.
