// Azure Container Apps hosting pro VolejbalApp (Web = Blazor WASM host + REST API v jednom).
// Deployment scope: resource group (předpokládá existující RG, viz deploy/README.md).
//
// Mimo scope této šablony:
// - databáze (hostovaná jinde) - připojení jen přes connection string parametr
// - migrace/seed databáze (MigrationTool) - řeší se mimo tuto šablonu
// - custom doména volejbal.kanda.eu - přidá se ručně po ověření provozu

@description('Lokace pro všechny resources.')
param location string = resourceGroup().location

@description('Název Container Apps Environment.')
param environmentName string = 'jk-volejbal-ca-env'

@description('Název Container App.')
param containerAppName string = 'jk-volejbal-ca-web'

@description('Plně kvalifikovaná reference na image, např. ghcr.io/<github-user>/volejbal-web:latest.')
param containerImage string

@description('Connection string k databázi (hostované mimo tuto šablonu).')
@secure()
param databaseConnectionString string

@description('Název Application Insights (vytváří tato šablona jako workspace-based nad Log Analytics workspace níže).')
param applicationInsightsName string = 'jk-volejbal-appinsights'

@description('Název Log Analytics workspace (sdílí ho Container Apps Environment pro logy kontejneru i Application Insights).')
param logAnalyticsName string = 'jk-volejbal-logs'

@description('Počet dní uchování logů v Log Analytics.')
param logAnalyticsRetentionDays int = 30

// Jako string kvůli json() níže - bicep nemá typ pro desetinná čísla (stejný důvod jako u cpu: json('0.25')).
@description('Denní strop ingestace do Log Analytics v GB. Pojistka proti utržené fakturaci, ne nástroj běžné optimalizace - při dosažení se sběr dat na zbytek dne ZASTAVÍ a přijdete o výhled na aplikaci (viz deploy/README.md). "-1" = bez limitu.')
param logAnalyticsDailyQuotaGb string = '0.25'

@description('ASP.NET Core prostředí (ovlivňuje appsettings.Web.{env}.json a chování aplikace).')
param aspNetCoreEnvironment string = 'Production'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: logAnalyticsName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018'
    }
    retentionInDays: logAnalyticsRetentionDays
    workspaceCapping: {
      dailyQuotaGb: json(logAnalyticsDailyQuotaGb)
    }
  }
}

resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: applicationInsightsName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    // Workspace-based App Insights - sdílí Log Analytics workspace s Container Apps Environment,
    // takže logy kontejneru i telemetrie aplikace končí na jednom místě.
    WorkspaceResourceId: logAnalytics.id
  }
}

resource containerAppsEnvironment 'Microsoft.App/managedEnvironments@2024-03-01' = {
  name: environmentName
  location: location
  properties: {
    appLogsConfiguration: {
      destination: 'log-analytics'
      logAnalyticsConfiguration: {
        customerId: logAnalytics.properties.customerId
        sharedKey: logAnalytics.listKeys().primarySharedKey
      }
    }
  }
}

resource webApp 'Microsoft.App/containerApps@2024-03-01' = {
  name: containerAppName
  location: location
  properties: {
    managedEnvironmentId: containerAppsEnvironment.id
    configuration: {
      activeRevisionsMode: 'Single'
      ingress: {
        external: true
        targetPort: 8080
        transport: 'auto'
        allowInsecure: false
      }
      // Žádný registries blok - image v ghcr.io je veřejný, ACA ho pullne anonymně.
      // Kdyby package někdy zprivátněl, je potřeba sem doplnit registries + secret s PAT.
      secrets: [
        {
          name: 'db-connectionstring'
          value: databaseConnectionString
        }
        {
          name: 'appinsights-connectionstring'
          value: appInsights.properties.ConnectionString
        }
      ]
    }
    template: {
      containers: [
        {
          name: 'web'
          image: containerImage
          resources: {
            cpu: json('0.25')
            memory: '0.5Gi'
          }
          env: [
            {
              name: 'ASPNETCORE_ENVIRONMENT'
              value: aspNetCoreEnvironment
            }
            {
              // Bez tohohle by za ACA ingress (TLS terminuje na edge, do kontejneru chodí HTTP)
              // způsobil app.UseHttpsRedirection() nekonečnou smyčku redirectů - viz deploy/README.md.
              name: 'ASPNETCORE_FORWARDEDHEADERS_ENABLED'
              value: 'true'
            }
            {
              name: 'TZ'
              value: 'Europe/Prague'
            }
            {
              name: 'ConnectionStrings__Database'
              secretRef: 'db-connectionstring'
            }
            {
              name: 'ApplicationInsights__ConnectionString'
              secretRef: 'appinsights-connectionstring'
            }
          ]
        }
      ]
      scale: {
        // minReplicas 0 = scale-to-zero. maxReplicas MUSÍ zůstat 1 - RecurringJobsBackgroundService
        // je in-process plánovač bez distribuovaného zámku; dvě repliky = duplicitní běhy jobů.
        minReplicas: 0
        maxReplicas: 1
      }
    }
  }
}

@description('Veřejná URL aplikace (azurecontainerapps.io, dokud není navázaná custom doména).')
output containerAppUrl string = 'https://${webApp.properties.configuration.ingress.fqdn}'
