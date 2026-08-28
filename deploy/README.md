# Nasazení na Azure Container Apps

Bicep šablona ([main.bicep](main.bicep)) pro hosting `Web` (Blazor WASM host + REST API v jedné aplikaci). Vytváří Log Analytics workspace, Application Insights, Container Apps Environment a Container App. Databáze a custom doména jsou mimo scope této šablony — viz komentář v hlavičce `main.bicep`.

| Resource | Typ | Název |
| --- | --- | --- |
| Log Analytics workspace | `Microsoft.OperationalInsights/workspaces` | `jk-volejbal-logs` |
| Application Insights | `Microsoft.Insights/components` | `jk-volejbal-appinsights` |
| Container Apps Environment | `Microsoft.App/managedEnvironments` | `jk-volejbal-ca-env` |
| Container App | `Microsoft.App/containerApps` | `jk-volejbal-ca-web` |

Application Insights je **workspace-based** nad tímtéž Log Analytics workspace, který používá Container Apps Environment pro logy kontejneru — telemetrie aplikace i logy tak končí na jednom místě. Connection string se do Container App předává referencí (`appInsights.properties.ConnectionString`), takže žádný GitHub secret pro něj není potřeba.

## Container image — CI ano, CD ne

`.github/workflows/build-production.yml` (workflow *Build Production*) běží na push do branch **`release/production`** (nebo ruční `workflow_dispatch`; PR trigger tam záměrně není) a kromě build+test zabuildí `Web` přes vestavěnou kontejnerizaci .NET SDK (`dotnet publish -t:PublishContainer`, žádný Dockerfile) a pushne na `ghcr.io/<github-user>/volejbal-web`. Tag je jen commit SHA — **záměrně žádný `latest`**: tenhle krok jen publikuje image, nic nedeployuje, a floating tag by byl implicitní deploy target bez záměrného rozhodnutí, co jde do produkce. Autentizace jde přes automatický `GITHUB_TOKEN` (`packages: write`), žádný extra secret v CI netřeba.

`.github/workflows/build.yml` (workflow *Build*, master + PR do masteru) dělá jen restore/build/test — nic nepublikuje ani nepushuje.

Package `volejbal-web` je v GHCR nastavený jako **veřejný**, takže ho Container App pullne anonymně — `main.bicep` proto nemá `registries` blok ani žádný secret s PAT. GHCR dává novým packages defaultně privátní viditelnost; kdyby se package někdy vrátil na privátní (nebo vznikl nový), pull by začal padat na `UNAUTHORIZED` a bylo by potřeba do šablony doplnit `registries` + secret s classic PAT se scope `read:packages`.

Skutečné nasazení (CD) je oddělený **ruční** workflow — viz níže.

## Nasazení — ruční workflow (`.github/workflows/deploy.yml`)

Standardní cesta: záložka *Actions* → *Deploy to Azure Container Apps* → *Run workflow*, zadat commit SHA image, který se má nasadit (najdete ho v běhu workflow *Build Production*, který image pushnul).

### Jednorázová příprava

1. Existující resource group (`az group create -n JkVolejbalRG -l westeurope`) — název musí sedět s `RESOURCE_GROUP` v [deploy.yml](../.github/workflows/deploy.yml).
2. Branch `release/production` v repozitáři — bez ní se *Build Production* nikdy nespustí a není co nasazovat.
3. Service principal pro `azure/login` s **federated credentials (OIDC)** — bez hesla, není co rotovat:
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
     "subject": "repo:jirikanda/VolejbalApp:environment:production",
     "audiences": ["api://AzureADTokenExchange"]
   }'

   # oprávnění na resource group
   az role assignment create --assignee "$APP_ID" --role Contributor \
     --scope "/subscriptions/$SUB_ID/resourceGroups/JkVolejbalRG"

   echo "AZURE_CLIENT_ID=$APP_ID"; echo "AZURE_TENANT_ID=$TENANT_ID"; echo "AZURE_SUBSCRIPTION_ID=$SUB_ID"
   ```

   > **Pozor na `subject`.** Job v `deploy.yml` má `environment: production`, a když job cílí na environment, GitHub do OIDC tokenu dá claim `repo:<owner>/<repo>:environment:production` — **ne** `ref:refs/heads/master`. Federated credential nastavený na branch by se nespároval a login by skončil na `AADSTS70021: No matching federated identity record found`. Kdyby z `deploy.yml` někdy zmizel `environment: production`, je potřeba federated credential přenastavit.

4. Secrets (`Settings → Secrets and variables → Actions`, případně jako environment secrets pod `production`, viz níže):
   - `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` — z výpisu výše (nejsou to tajemství v pravém smyslu, ale držíme je jednotně mezi secrets).
   - `DATABASE_CONNECTION_STRING` — connection string k databázi (hostované mimo tuto šablonu). Jediný secret s tajemstvím — GHCR pull i Azure login žádné heslo nepotřebují.
5. Volitelně: v *Settings → Environments* vytvořit environment `production` a přidat *required reviewers* — pak `deploy.yml` (má `environment: production`) počká na schválení, než doopravdy nasadí.

Výstup workflow (`containerAppUrl`) je veřejná adresa (`https://<app>.<region>.azurecontainerapps.io`), dokud není navázaná custom doména.

### Ruční nasazení bez GitHub Actions (alternativa)

```bash
az deployment group create \
  --resource-group JkVolejbalRG \
  --template-file deploy/main.bicep \
  --parameters \
    containerImage='ghcr.io/<github-user>/volejbal-web:<commit-sha>' \
    databaseConnectionString='<connection string>'
```

### Aktualizace image (redeploy)

Opakované spuštění workflow (nebo `az deployment group create` výše) s jiným `imageTag`/`containerImage` vytvoří novou revizi Container App a přepne na ni provoz. Bez potřeby měnit secrets, pokud se connection stringy nezměnily.

## Poznámky

- **`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`** je nutný — ACA ingress terminuje TLS a do kontejneru posílá HTTP; bez tohohle by `app.UseHttpsRedirection()` (viz [Web/Startup.cs](../Web/Startup.cs)) způsobil nekonečnou smyčku redirectů, protože by appka nerozpoznala, že originální request byl HTTPS.
- **`maxReplicas: 1` je závazné, ne jen výchozí hodnota** — [Services/Jobs/RecurringJobsBackgroundService.cs](../Services/Jobs/RecurringJobsBackgroundService.cs) je in-process plánovač bez distribuovaného zámku; dvě repliky by znamenaly, že se joby (EnsureTerminy, DeaktivaceOsob) spouští duplicitně.
- **`minReplicas: 0`** (scale-to-zero) znamená, že po period nečinnosti aplikace "usne" a další request ji probudí (cold start). [Web/Infrastructure/WarmupBackgroundService.cs](../Web/Infrastructure/WarmupBackgroundService.cs) po startu sám zavolá klíčové endpointy, aby EF Core/MVC pipeline byly zahřáté ještě před prvním reálným requestem.
- Migrace databázového schématu a data seedy řeší samostatný konzolový **`MigrationTool`** projekt (mimo scope téhle šablony). *Build Production* ho publikuje jako artefakt `MigrationTool` (linux-x64, framework-dependent), ale **žádná pipeline ho nespouští** — aplikace schéma za běhu nemigruje, takže před prvním startem a před každým deployem s novou migrací musí někdo artefakt stáhnout a spustit proti produkčnímu connection stringu:
  ```bash
  ./KandaEu.Volejbal.MigrationTool --connectionstring "<connection string>"
  ```
