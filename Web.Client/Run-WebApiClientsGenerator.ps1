# Vygeneruje typované API klienty z OpenAPI dokumentu hostu (Web) do _generated/WebApiClients.cs.
# Spouštět z adresáře Web.Client. Vygenerovaný soubor je součástí gitu - po změně API je potřeba ho přegenerovat a commitnout.
#
# Pozn.: Build hostu vyžaduje kompilovatelný Web.Client se STARÝM vygenerovaným kódem. Pokud breaking změna
# v Contracts rozbije starý vygenerovaný kód, je potřeba dotčený kód dočasně opravit ručně a generování zopakovat.

$ErrorActionPreference = 'Stop'

& dotnet tool restore
if ($LASTEXITCODE -ne 0) { throw 'dotnet tool restore se nezdařilo.' }

# Build hostu vygeneruje OpenAPI dokument do ..\Web\obj\Web_current.json (OpenApiGenerateDocumentsOnBuild=true).
& dotnet build ..\Web\Web.csproj
if ($LASTEXITCODE -ne 0) { throw 'Build projektu Web se nezdařil.' }

& dotnet nswag openapi2csclient `
	/input:..\Web\obj\Web_current.json `
	/output:_generated\WebApiClients.cs `
	/namespace:KandaEu.Volejbal.Web.Client.WebApiClients `
	/className:"{controller}WebApiClient" `
	/generateClientInterfaces:true `
	/generateDtoTypes:false `
	/useBaseUrl:false `
	/jsonLibrary:SystemTextJson `
	/additionalNamespaceUsages:KandaEu.Volejbal.Contracts.Nastenka.Dto,KandaEu.Volejbal.Contracts.Osoby.Dto,KandaEu.Volejbal.Contracts.Reporty.Dto,KandaEu.Volejbal.Contracts.Terminy.Dto
if ($LASTEXITCODE -ne 0) { throw 'NSwag generování klientů se nezdařilo.' }

# NSwag zapisuje výstup bez BOM - sjednocujeme s konvencí repa (UTF-8 s BOM).
$outputPath = Join-Path $PSScriptRoot '_generated\WebApiClients.cs'
$outputText = [System.IO.File]::ReadAllText($outputPath)
[System.IO.File]::WriteAllText($outputPath, $outputText, [System.Text.UTF8Encoding]::new($true))
