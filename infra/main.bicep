// Azure Functions (Flex Consumption) hosting pro VolejbalApp Api (REST API) + Azure Static Web Apps
// hosting pro Web.Client (Blazor WASM frontend, samostatně nasazovaný, viz deploy.yml job deploy-frontend).
// Deployment scope: resource group (předpokládá existující RG, viz infra/README.md).
//
// Proč Functions místo Container Apps: na ACA se scale-to-zero byl naměřený studený start 33 s, z toho
// podstatnou část tvořil pull image z ghcr.io (mimo Azure) a náběh repliky. Flex Consumption nasazuje
// zip do blob containeru ve stejném regionu, takže odpadá pull kontejneru z cizího registru.
//
// Mimo scope této šablony:
// - databáze (hostovaná jinde) - připojení jen přes connection string parametr
// - migrace/seed databáze (MigrationTool) - řeší se mimo tuto šablonu, ručně před deployem
// - doména pro Api - běží jen na defaultním *.azurewebsites.net hostname
//
// Custom doména volejbal.kanda.eu (binding + automatický cert) JE součástí šablony, ale patří
// Static Web App (frontend), ne Function App - řídí ji proměnná bindCustomDomain (zatím false,
// protože DNS ještě neukazuje na SWA) - viz komentář u ní a infra/README.md pro dvoufázový postup.

@description('Lokace pro všechny resources.')
param location string = resourceGroup().location

@description('Název Function App.')
param functionAppName string = 'jk-volejbal-func'

@description('Název App Service plánu (Flex Consumption). Na Flex platí jedna aplikace na plán.')
param functionPlanName string = 'jk-volejbal-func-plan'

@description('Connection string k databázi (hostované mimo tuto šablonu).')
@secure()
param databaseConnectionString string

@description('Název Application Insights (vytváří tato šablona jako workspace-based nad Log Analytics workspace níže).')
param applicationInsightsName string = 'jk-volejbal-appinsights'

@description('Název Log Analytics workspace (sdílí ho Application Insights).')
param logAnalyticsName string = 'jk-volejbal-logs'

@description('Počet dní uchování logů v Log Analytics.')
param logAnalyticsRetentionDays int = 30

// Jako string kvůli json() níže - bicep nemá typ pro desetinná čísla.
@description('Denní strop ingestace do Log Analytics v GB. Pojistka proti utržené fakturaci, ne nástroj běžné optimalizace - při dosažení se sběr dat na zbytek dne ZASTAVÍ a přijdete o výhled na aplikaci (viz infra/README.md). "-1" = bez limitu.')
param logAnalyticsDailyQuotaGb string = '0.25'

@description('Prostředí aplikace (ovlivňuje appsettings.Api.{env}.json a chování aplikace).')
param azureFunctionsEnvironment string = 'Production'

@description('Velikost paměti instance v MB. 512 = 0,25 core, což odpovídá dosavadnímu ACA kontejneru.')
@allowed([
  512
  2048
  4096
])
param instanceMemoryMB int = 512

@description('Strop počtu on-demand instancí. Nahrazuje dřívější rate limiter "DefaultAPI" jako pojistku proti přetížení databáze - aplikace je psaná pro malý provoz a databáze je sdílená.')
param maximumInstanceCount int = 5

// Počet always-ready instancí. Nula = plná serverless ekonomika (vejde se do free grantu), ale platí se
// za ni studeným startem v jednotkách sekund. Jedna instance ho prakticky odstraní za ~6,5 USD/měsíc
// (baseline 0,000005 USD/GB-s, na always-ready se free granty NEvztahují). Zapnout až podle měření
// skutečného studeného startu, viz infra/README.md.
@description('Počet always-ready instancí (0 = vypnuto).')
param alwaysReadyInstanceCount int = 0

// Název Static Web App (frontend Web.Client, nasazovaný samostatně přes deploy.yml job deploy-frontend).
var staticWebAppName = 'JkVolejbalSWA'

// Lokace Static Web App - NEZÁVISLÁ na parametru location (Germany West Central).
// Azure Static Web Apps v Germany West Central není dostupné, nejbližší podporovaný region je West Europe.
var staticWebAppLocation = 'westeurope'

// Vlastní doména vázaná na Static Web App (frontend).
var customDomainName = 'volejbal.kanda.eu'

// Zapíná binding custom domény na Static Web App. Vyžaduje, aby DNS CNAME volejbal.kanda.eu už
// ukazoval na staticWebApp.properties.defaultHostname DŘÍV, než se tenhle resource nasadí - SWA
// validaci (cname-delegation) i vydání certifikátu dělá automaticky, ale až v okamžiku, kdy CNAME
// skutečně existuje. Dvoufázový postup (viz infra/README.md): nasadit s false, ověřit deploy-frontend
// job, přepsat DNS, tady přepnout na true a nasadit znovu.
var bindCustomDomain = false

// Storage account pro Functions: drží interní metadata hostu (AzureWebJobsStorage, včetně zámků
// timer triggeru) a zároveň slouží jako úložiště deployment balíčku. Název musí být globálně unikátní
// a smí obsahovat jen malá písmena a číslice, proto uniqueString místo čitelného jména.
var storageAccountName = 'jkvolejbal${uniqueString(resourceGroup().id)}'
var deploymentContainerName = 'deployment-package'

// Vestavěné role pro přístup Function App ke storage přes managed identity (bez klíčů).
var storageBlobDataOwnerRoleId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
var storageQueueDataContributorRoleId = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
var storageTableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2025-07-01' = {
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
    WorkspaceResourceId: logAnalytics.id
  }
}

resource storageAccount 'Microsoft.Storage/storageAccounts@2025-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2025-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource deploymentContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2025-01-01' = {
  parent: blobService
  name: deploymentContainerName
  properties: {
    publicAccess: 'None'
  }
}

// Static Web App (frontend Web.Client) - obsah nasazuje samostatný GH Actions job (deploy-frontend),
// ne nativní SWA<->GitHub integrace, proto žádný repositoryUrl/branch/buildProperties.
resource staticWebApp 'Microsoft.Web/staticSites@2025-05-01' = {
  name: staticWebAppName
  location: staticWebAppLocation
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// Podmíněný na bindCustomDomain - SWA ověřuje vlastnictví domény přes DNS (viz komentář u parametru),
// takže dokud CNAME neukazuje na staticWebApp, resource se vůbec nemá zkoušet vytvořit.
resource staticWebAppCustomDomain 'Microsoft.Web/staticSites/customDomains@2025-05-01' = if (bindCustomDomain) {
  parent: staticWebApp
  name: customDomainName
  properties: {
    validationMethod: 'cname-delegation'
  }
}

resource functionPlan 'Microsoft.Web/serverfarms@2024-04-01' = {
  name: functionPlanName
  location: location
  kind: 'functionapp'
  sku: {
    name: 'FC1'
    tier: 'FlexConsumption'
  }
  properties: {
    reserved: true // Flex Consumption je jen linuxový
  }
}

resource functionApp 'Microsoft.Web/sites@2024-04-01' = {
  name: functionAppName
  location: location
  kind: 'functionapp,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: functionPlan.id
    httpsOnly: true
    functionAppConfig: {
      deployment: {
        storage: {
          type: 'blobContainer'
          value: '${storageAccount.properties.primaryEndpoints.blob}${deploymentContainerName}'
          authentication: {
            type: 'SystemAssignedIdentity'
          }
        }
      }
      scaleAndConcurrency: {
        instanceMemoryMB: instanceMemoryMB
        maximumInstanceCount: maximumInstanceCount
        // Prázdné pole = žádná always-ready instance, aplikace plně škáluje k nule.
        alwaysReady: (alwaysReadyInstanceCount > 0) ? [
          {
            name: 'http'
            instanceCount: alwaysReadyInstanceCount
          }
        ] : []
      }
      runtime: {
        name: 'dotnet-isolated'
        version: '10.0'
      }
    }
    siteConfig: {
      // CORS allow-list pro Web.Client (SWA). Řeší ho platforma, ne aplikace - Azure Functions
      // nezpřístupňují ASP.NET Core middleware pipeline, takže UseCors() tu není k dispozici.
      // Defaultní hostname SWA je vždy platný cíl, custom doména (viz staticWebAppCustomDomain) se
      // přidává preventivně i před vlastním DNS cutoverem.
      cors: {
        allowedOrigins: [
          'https://${staticWebApp.properties.defaultHostname}'
          'https://${customDomainName}'
        ]
        supportCredentials: false
      }
      appSettings: [
        {
          name: 'AzureWebJobsStorage__blobServiceUri'
          value: storageAccount.properties.primaryEndpoints.blob
        }
        {
          name: 'AzureWebJobsStorage__queueServiceUri'
          value: storageAccount.properties.primaryEndpoints.queue
        }
        {
          name: 'AzureWebJobsStorage__tableServiceUri'
          value: storageAccount.properties.primaryEndpoints.table
        }
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'AZURE_FUNCTIONS_ENVIRONMENT'
          value: azureFunctionsEnvironment
        }
        {
          name: 'ConnectionStrings__Database'
          value: databaseConnectionString
        }
        // Pozor: TZ (ani WEBSITE_TIME_ZONE) se nenastavuje - Flex Consumption je nepodporuje a tiše
        // ignoruje. Časovou zónu drží aplikace v kódu, viz Services/Infrastructure/TimeService.
      ]
    }
  }
  dependsOn: [
    deploymentContainer
  ]
}

// Přístup ke storage přes managed identity místo klíčů. Bez těchto rolí se host nerozběhne:
// blob drží deployment balíček i zámky timer triggeru, queue a table interní metadata hostu.
resource storageBlobDataOwnerAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionApp.id, storageBlobDataOwnerRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageBlobDataOwnerRoleId)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageQueueDataContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionApp.id, storageQueueDataContributorRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageQueueDataContributorRoleId)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

resource storageTableDataContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: storageAccount
  name: guid(storageAccount.id, functionApp.id, storageTableDataContributorRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageTableDataContributorRoleId)
    principalId: functionApp.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

@description('Veřejná URL Api - defaultní azurewebsites.net hostname, žádná custom doména se sem neváže. Musí souhlasit s ApiBaseUrl ve Web.Client/wwwroot/appsettings.json.')
output functionAppUrl string = 'https://${functionApp.properties.defaultHostName}'

@description('Název Function App - čte ho deploy job při nasazení balíčku, aby název nemusel být napsaný zvlášť i ve workflow.')
output functionAppName string = functionApp.name

@description('Defaultní hostname Static Web App (frontend) - veřejná URL frontendu, dokud nemá custom doménu, a cíl DNS CNAME při nastavování bindCustomDomain (viz infra/README.md).')
output staticWebAppDefaultHostname string = staticWebApp.properties.defaultHostname

@description('Název Static Web App - čte ho deploy-frontend job, aby název nemusel být napsaný zvlášť i ve workflow.')
output staticWebAppName string = staticWebApp.name
