// Azure Functions (Flex Consumption) hosting pro VolejbalApp Api (REST API) + Azure Static Web Apps
// hosting pro Web.Client (Blazor WASM frontend, samostatně nasazovaný, viz deploy.yml job deploy-frontend).
// Deployment scope: resource group (předpokládá existující RG, viz infra/README.md).
//
// Proč Functions místo Container Apps: na ACA se scale-to-zero byl naměřený studený start 33 s, z toho
// podstatnou část tvořil pull image z ghcr.io (mimo Azure) a náběh repliky. Flex Consumption nasazuje
// zip do blob containeru ve stejném regionu, takže odpadá pull kontejneru z cizího registru.
//
// Databáze (Cosmos DB, free tier) JE součástí šablony včetně kontejnerů - datové úložiště tím přestává
// být cizím resourcem a z pipeline mizí poslední skutečný secret: aplikace se autentizuje managed
// identitou přes datové RBAC (viz cosmosDataContributorAssignment), ne klíčem.
//
// Mimo scope této šablony:
// - doména pro Api - běží jen na defaultním *.azurewebsites.net hostname
//
// Custom doména volejbal.kanda.eu (binding + automatický cert) JE součástí šablony, ale patří
// Static Web App (frontend), ne Function App - řídí ji proměnná bindCustomDomain (zatím false,
// protože DNS ještě neukazuje na SWA) - viz komentář u ní a infra/README.md pro dvoufázový postup.

@description('Lokace pro všechny resources.')
param location string = resourceGroup().location

@description('Název Function App.')
param functionAppName string = 'JkVolejbalFunc'

@description('Název App Service plánu (Flex Consumption). Na Flex platí jedna aplikace na plán.')
param functionPlanName string = 'JkVolejbalFuncPlan'

@description('Název účtu Cosmos DB. Musí být globálně unikátní, jen malá písmena, číslice a pomlčky.')
param cosmosAccountName string = 'jkvolejbalcosmosdb'

@description('Název databáze v Cosmos DB.')
param cosmosDatabaseName string = 'volejbal'

// Free tier: 1000 RU/s a 25 GB zdarma, ale POUZE JEDEN účet na subscription a zapnout ho lze jen při
// zakládání účtu (pozdější změna vyžaduje účet smazat a založit znovu). Objem aplikace je proti tomu
// zanedbatelný - jednotky MB a desítky dokumentů na sezónu.
@description('Zapíná free tier účtu Cosmos DB.')
param cosmosEnableFreeTier bool = true

// Propustnost sdílená celou databází, na minimu: 400 RU/s je nejmenší povolená hodnota pro sdílenou
// databázi a stačí až pro 4 kontejnery (Cosmos vyžaduje 100 RU/s na kontejner; pátý kontejner by
// znamenal 500). Tři samostatně provisionované kontejnery by chtěly 3x400 RU/s a do free tier by se
// nevešly, proto sdílená. Zbytek free tier grantu (viz cosmosTotalThroughputLimit) zůstává volný
// pro další databázi na tomtéž účtu.
@description('Propustnost databáze v RU/s (sdílená všemi kontejnery). Minimum je 400.')
param cosmosThroughput int = 400

// Strop celkové propustnosti účtu = free tier grant (1000 RU/s zdarma). Pojistka proti tomu, aby
// někdo v portálu omylem přidal placenou propustnost - všechno nad strop účet odmítne.
@description('Strop celkové provisionované propustnosti účtu v RU/s (součet všech databází a kontejnerů).')
param cosmosTotalThroughputLimit int = 1000

@description('Název Application Insights (vytváří tato šablona jako workspace-based nad Log Analytics workspace níže).')
param applicationInsightsName string = 'JkVolejbalAppInsights'

@description('Název Log Analytics workspace (sdílí ho Application Insights).')
param logAnalyticsName string = 'JkVolejbalLAW'

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

@description('Strop počtu on-demand instancí. Hlavně cenová pojistka: při zahlcení API (anonymní, bez rate limitingu) se platí nejvýš jedna instance. Jedna instance s 0,25 core pokryje provoz aplikace s velkou rezervou a aplikace na počtu instancí nezávisí (viz infra/README.md). Před databází chrání spíš strop propustnosti Cosmosu.')
param maximumInstanceCount int = 1

// Počet always-ready instancí. Nula = plná serverless ekonomika (vejde se do free grantu), ale platí se
// za ni studeným startem v jednotkách sekund. Jedna instance ho prakticky odstraní za ~6,5 USD/měsíc
// (baseline 0,000005 USD/GB-s, na always-ready se free granty NEvztahují). Zapnout až podle měření
// skutečného studeného startu, viz infra/README.md.
@description('Počet always-ready instancí (0 = vypnuto).')
param alwaysReadyInstanceCount int = 0

// Hlídání nákladů: API je anonymní a bez rate limitingu, takže zahlcení se platí za každé spuštění funkce
// (maximumInstanceCount omezuje jen čas instance, ne počet spuštění). Alert na počet spuštění aplikaci
// dočasně zastaví (circuitBreaker), rozpočet jen upozorní.
@description('E-mail pro alerty (action group) a upozornění rozpočtu.')
param alertEmail string = 'kanda@havit.cz'

// Běžný provoz je v jednotkách requestů za minutu; 3000 za 5 minut je průměrně 10 req/s.
@description('Práh alertu na počet spuštění funkcí za 5 minut.')
param executionCountAlertThreshold int = 3000

@description('Měsíční rozpočet resource group v USD; upozornění chodí při dosažení 100 %.')
param budgetAmount int = 5

// Začátek rozpočtu musí být první den měsíce. Pevná hodnota, ne utcNow(): šablona se nasazuje celá při
// každém deployi a posouvání začátku by rozpočet pokaždé měnilo.
@description('Začátek období rozpočtu (první den měsíce, formát yyyy-MM-dd).')
param budgetStartDate string = '2026-10-01'

@description('Za kolik minut pojistka (circuitBreaker) Function App po zastavení znovu spustí.')
@minValue(1)
param circuitBreakerRestartMinutes int = 30

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
// a smí obsahovat jen malá písmena a číslice.
var storageAccountName = 'jkvolejbalfuncstorage'
var deploymentContainerName = 'deployment-package'

// Vestavěné role pro přístup Function App ke storage přes managed identity (bez klíčů).
var storageBlobDataOwnerRoleId = 'b7e6dc6d-f1e8-4753-8033-0f276bb0955b'
var storageQueueDataContributorRoleId = '974c5e8b-45b9-4653-ba55-5f855dd0fb88'
var storageTableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

// Vestavěná datová role Cosmos DB "Built-in Data Contributor" (čtení i zápis dokumentů, nikoli správa
// účtu). Pozor, tohle je DATOVÉ RBAC Cosmosu, ne Azure RBAC - přiřazuje se resourcem
// Microsoft.DocumentDB/.../sqlRoleAssignments a z portálu se nastavit nedá, jen šablonou nebo CLI.
var cosmosDataContributorRoleId = '00000000-0000-0000-0000-000000000002'

// Vestavěná role "Website Contributor" - dovoluje Logic App pojistky (viz circuitBreaker) zastavit a
// spustit Function App, ale ne měnit nic jiného v resource group.
var websiteContributorRoleId = 'de139f84-1756-47ae-9be6-808fbbe84772'

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

// Cosmos DB - datové úložiště aplikace. Klíčový přístup zůstává zapnutý kvůli Data Exploreru
// v portálu; aplikace ho nepoužívá, autentizuje se managed identitou (viz cosmosDataContributorAssignment).
resource cosmosAccount 'Microsoft.DocumentDB/databaseAccounts@2024-11-15' = {
  name: cosmosAccountName
  location: location
  kind: 'GlobalDocumentDB'
  properties: {
    databaseAccountOfferType: 'Standard'
    enableFreeTier: cosmosEnableFreeTier
    minimalTlsVersion: 'Tls12'
    // Účet vznikl ručně v portálu ještě před touto šablonou; následující hodnoty odpovídají jeho stavu.
    // Continuous backup (7 dní) je u free tier zdarma a zpět na periodický se přepnout nedá -
    // šablona bez backupPolicy by se o to pokusila a nasazení by spadlo.
    backupPolicy: {
      type: 'Continuous'
      continuousModeProperties: {
        tier: 'Continuous7Days'
      }
    }
    enableAutomaticFailover: true
    // Strop celkové propustnosti účtu, viz komentář u parametru.
    capacity: {
      totalThroughputLimit: cosmosTotalThroughputLimit
    }
    // Session consistency je default a pro jednu repliku i jediný rozumný kompromis: čtení po zápisu
    // ze stejné session vidí vlastní zápisy, což je přesně to, co UI po přihlášení na termín očekává.
    consistencyPolicy: {
      defaultConsistencyLevel: 'Session'
    }
    locations: [
      {
        locationName: location
        failoverPriority: 0
        isZoneRedundant: false
      }
    ]
  }
}

resource cosmosDatabase 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases@2024-11-15' = {
  parent: cosmosAccount
  name: cosmosDatabaseName
  properties: {
    resource: {
      id: cosmosDatabaseName
    }
    options: {
      throughput: cosmosThroughput
    }
  }
}

// Všechny kontejnery mají partition key /id: model tak nenese žádnou vlastnost jen kvůli Cosmosu a point
// read i zápis mají klíč vždy v ruce. Seznamy běží napříč partitions, ale celá data leží v jediné
// fyzické partition, takže je fan-out zdarma.
resource cosmosOsobyContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-11-15' = {
  parent: cosmosDatabase
  name: 'osoby'
  properties: {
    resource: {
      id: 'osoby'
      partitionKey: {
        paths: [ '/id' ]
        kind: 'Hash'
      }
    }
  }
}

// Termíny mají partition key /id a id dokumentu je datum termínu, takže každý termín je vlastní logickou
// partition. Unikátnost id v partition tím nahrazuje dřívější index UIDX_Termin_Datum_Deleted; partition
// key odvozený z data (sezóna) by nic nepřinesl - seznamy se čtou napříč sezónami tak jako tak. Přihlášky jsou vnořené
// v dokumentu a z indexu vyloučené: dovnitř pole se nikdy nedotazujeme, ale každé přihlášení dokument
// přepíše - bez vyloučení by se při nejčastější operaci aplikace reindexovalo celé pole.
resource cosmosTerminyContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-11-15' = {
  parent: cosmosDatabase
  name: 'terminy'
  properties: {
    resource: {
      id: 'terminy'
      partitionKey: {
        paths: [ '/id' ]
        kind: 'Hash'
      }
      indexingPolicy: {
        indexingMode: 'consistent'
        automatic: true
        includedPaths: [ { path: '/*' } ]
        excludedPaths: [
          { path: '/prihlasky/*' }
          { path: '/"_etag"/?' }
        ]
      }
    }
  }
}

resource cosmosVzkazyContainer 'Microsoft.DocumentDB/databaseAccounts/sqlDatabases/containers@2024-11-15' = {
  parent: cosmosDatabase
  name: 'vzkazy'
  properties: {
    resource: {
      id: 'vzkazy'
      partitionKey: {
        paths: [ '/id' ]
        kind: 'Hash'
      }
    }
  }
}

// Static Web App (frontend Web.Client) - obsah nasazuje samostatný GH Actions job (deploy-frontend),
// ne nativní SWA<->GitHub integrace, proto žádný repositoryUrl/branch/buildProperties.
resource staticWebApp 'Microsoft.Web/staticSites@2025-03-01' = {
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
resource staticWebAppCustomDomain 'Microsoft.Web/staticSites/customDomains@2025-03-01' = if (bindCustomDomain) {
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
          name: 'Cosmos__Endpoint'
          value: cosmosAccount.properties.documentEndpoint
        }
        {
          name: 'Cosmos__DatabaseId'
          value: cosmosDatabaseName
        }
        // Cosmos__Key se ZÁMĚRNĚ nenastavuje: prázdný klíč znamená přihlášení managed identitou
        // (viz DataLayer/Cosmos/CosmosClientFactory.cs). Díky tomu v nasazení není žádné tajemství.
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

// Datový přístup Function App k Cosmosu. Bez tohoto přiřazení aplikace nastartuje, ale každý dotaz
// skončí na 403 - datové RBAC Cosmosu je nezávislé na Azure RBAC a role jako Contributor nad účtem
// na data nestačí.
resource cosmosDataContributorAssignment 'Microsoft.DocumentDB/databaseAccounts/sqlRoleAssignments@2024-11-15' = {
  parent: cosmosAccount
  name: guid(cosmosAccount.id, functionApp.id, cosmosDataContributorRoleId)
  properties: {
    roleDefinitionId: '${cosmosAccount.id}/sqlRoleDefinitions/${cosmosDataContributorRoleId}'
    principalId: functionApp.identity.principalId
    scope: cosmosAccount.id
  }
}

// Pojistka proti zahlcení: alert na počet spuštění (executionCountAlert) ji přes action group zavolá,
// ona Function App zastaví (POST .../stop), počká circuitBreakerRestartMinutes a zase ji spustí
// (POST .../start). Zastavenou aplikaci odmítá platforma dřív, než request dojde k instanci, takže se
// nic neplatí. Trvá-li útok, alert po startu vystřelí znovu a cyklus se opakuje - každé kolo stojí
// jen minuty floodu, než se alert vyhodnotí.
//
// Reaguje jen na monitorCondition 'Fired': alert má autoMitigate, takže po zastavení aplikace metrika
// spadne, alert se vyřeší a action group zavolá Logic App znovu se stavem 'Resolved' - bez podmínky by
// druhý běh aplikaci zastavil podruhé. Z téhož důvodu se start nedá navázat na vyřešení alertu
// (aplikace by naběhla hned po zastavení), proto pevné čekání.
//
// concurrency.runs = 1: běhy jdou za sebou, takže se stop a start dvou běhů nemohou proplést.
resource circuitBreaker 'Microsoft.Logic/workflows@2019-05-01' = {
  name: 'JkVolejbalCircuitBreaker'
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    state: 'Enabled'
    definition: {
      '$schema': 'https://schema.management.azure.com/providers/Microsoft.Logic/schemas/2016-06-01/workflowdefinition.json#'
      contentVersion: '1.0.0.0'
      triggers: {
        manual: {
          type: 'Request'
          kind: 'Http'
          runtimeConfiguration: {
            concurrency: {
              runs: 1
            }
          }
        }
      }
      actions: {
        JenPriSpusteniAlertu: {
          type: 'If'
          runAfter: {}
          expression: {
            and: [
              {
                equals: [
                  '@triggerBody()?[\'data\']?[\'essentials\']?[\'monitorCondition\']'
                  'Fired'
                ]
              }
            ]
          }
          actions: {
            Zastavit: {
              type: 'Http'
              runAfter: {}
              inputs: {
                method: 'POST'
                uri: '${environment().resourceManager}${skip(functionApp.id, 1)}/stop?api-version=2024-04-01'
                authentication: {
                  type: 'ManagedServiceIdentity'
                  audience: environment().resourceManager
                }
              }
            }
            Pockat: {
              type: 'Wait'
              runAfter: {
                Zastavit: [
                  'Succeeded'
                ]
              }
              inputs: {
                interval: {
                  count: circuitBreakerRestartMinutes
                  unit: 'Minute'
                }
              }
            }
            Spustit: {
              type: 'Http'
              runAfter: {
                Pockat: [
                  'Succeeded'
                ]
              }
              inputs: {
                method: 'POST'
                uri: '${environment().resourceManager}${skip(functionApp.id, 1)}/start?api-version=2024-04-01'
                authentication: {
                  type: 'ManagedServiceIdentity'
                  audience: environment().resourceManager
                }
              }
            }
          }
          else: {
            actions: {}
          }
        }
      }
      outputs: {}
    }
  }
}

resource circuitBreakerWebsiteContributorAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  scope: functionApp
  name: guid(functionApp.id, circuitBreaker.id, websiteContributorRoleId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', websiteContributorRoleId)
    principalId: circuitBreaker.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

// Action group je globální resource (location 'global'). Používá ji alert (e-mail + pojistka);
// rozpočet posílá e-mail přímo (viz níže).
resource alertActionGroup 'Microsoft.Insights/actionGroups@2023-01-01' = {
  name: 'JkVolejbalAlerts'
  location: 'global'
  properties: {
    groupShortName: 'Volejbal' // max. 12 znaků, objevuje se v SMS/e-mailu
    enabled: true
    emailReceivers: [
      {
        name: 'email'
        emailAddress: alertEmail
        useCommonAlertSchema: true
      }
    ]
    // Common alert schema je nutné - podmínka v circuitBreaker čte data.essentials.monitorCondition.
    logicAppReceivers: [
      {
        name: 'circuitBreaker'
        resourceId: circuitBreaker.id
        callbackUrl: listCallbackUrl('${circuitBreaker.id}/triggers/manual', '2019-05-01').value
        useCommonAlertSchema: true
      }
    ]
  }
}

// Alert na počet spuštění funkcí - OnDemandFunctionExecutionCount je přesně ta metrika, ze které se
// počítá faktura za spuštění (jen Flex Consumption). Pošle e-mail a spustí pojistku (circuitBreaker),
// která Function App na circuitBreakerRestartMinutes zastaví. Metrika může chodit se zpožděním.
resource executionCountAlert 'Microsoft.Insights/metricAlerts@2018-03-01' = {
  name: 'JkVolejbalFuncExecutionCount'
  location: 'global'
  properties: {
    description: 'Neobvykle mnoho spuštění funkcí - možné zahlcení API (každé spuštění se platí).'
    severity: 2
    enabled: true
    scopes: [
      functionApp.id
    ]
    // Klouzavé okno 5 minut vyhodnocované každou minutu: práh zůstává "za 5 minut", ale pojistka
    // zareaguje až o 4 minuty dřív než při vyhodnocení po 5 minutách.
    evaluationFrequency: 'PT1M'
    windowSize: 'PT5M'
    autoMitigate: true
    criteria: {
      'odata.type': 'Microsoft.Azure.Monitor.SingleResourceMultipleMetricCriteria'
      allOf: [
        {
          criterionType: 'StaticThresholdCriterion'
          name: 'ExecutionCount'
          metricNamespace: 'Microsoft.Web/sites'
          metricName: 'OnDemandFunctionExecutionCount'
          operator: 'GreaterThan'
          threshold: executionCountAlertThreshold
          timeAggregation: 'Total'
        }
      ]
    }
    actions: [
      {
        actionGroupId: alertActionGroup.id
      }
    ]
  }
}

// Rozpočet resource group - pomalá pojistka nad skutečnými náklady (data o nákladech chodí se
// zpožděním hodin). Jen upozorní, nic nezastaví; tvrdý strop na Pay-As-You-Go neexistuje.
resource budget 'Microsoft.Consumption/budgets@2024-08-01' = {
  name: 'JkVolejbalBudget'
  properties: {
    category: 'Cost'
    amount: budgetAmount
    timeGrain: 'Monthly'
    timePeriod: {
      startDate: budgetStartDate
    }
    notifications: {
      actual100: {
        enabled: true
        operator: 'GreaterThanOrEqualTo'
        threshold: 100
        thresholdType: 'Actual'
        // contactEmails je povinné (contactGroups ne), proto přímo e-mail, ne action group - obojí
        // zároveň by poslalo upozornění dvakrát.
        contactEmails: [
          alertEmail
        ]
      }
    }
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

@description('Endpoint Cosmos DB - zadává se MigrationToolu při ručním převodu dat (viz infra/README.md).')
output cosmosEndpoint string = cosmosAccount.properties.documentEndpoint
