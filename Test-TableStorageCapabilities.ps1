#Requires -Version 7.0
<#
.SYNOPSIS
	Ukázka, co Azure Table Storage umí a neumí, na živém úložišti: vnořené objekty, pole, typy,
	dotazy do JSON stringu, chybějící vlastnosti, řazení, batch, ETag, limity.

.DESCRIPTION
	Volá Table REST API přímo (bez SDK), aby bylo vidět chování služby, ne knihovny. Každý případ
	vypíše očekávání, skutečný HTTP status a verdikt. Výchozí cíl je Azurite (účet devstoreaccount1).

	POZOR: Azurite nehlídá všechny limity (velikost vlastnosti, počet vlastností, velikost entity).
	Pro ty případy je potřeba reálný účet; skript to u každého z nich vypíše.

.EXAMPLE
	# Azurite (npx azurite-table nebo docker run -p 10002:10002 mcr.microsoft.com/azure-storage/azurite)
	./Test-TableStorageCapabilities.ps1

.EXAMPLE
	# Reálný účet přes klíč (účet musí mít povolený Shared Key access)
	./Test-TableStorageCapabilities.ps1 -AccountName mujucet -AccountKey (az storage account keys list -n mujucet --query '[0].value' -o tsv)

.EXAMPLE
	# Reálný účet přes SAS token (az storage account generate-sas --services t --resource-types sco --permissions racwdlu ...)
	./Test-TableStorageCapabilities.ps1 -AccountName mujucet -SasToken 'sv=2024-...&sig=...'
#>
[CmdletBinding()]
param(
	[string] $AccountName = 'devstoreaccount1',
	[string] $AccountKey = 'Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==',
	[string] $SasToken,
	[string] $Endpoint,          # výchozí: Azurite http://127.0.0.1:10002/devstoreaccount1, jinak https://{účet}.table.core.windows.net
	[string] $TableName = 'capabilitytest' + (Get-Date -Format 'HHmmss')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

if (-not $Endpoint)
{
	$Endpoint = if ($AccountName -eq 'devstoreaccount1') { 'http://127.0.0.1:10002/devstoreaccount1' } else { "https://$AccountName.table.core.windows.net" }
}
$Endpoint = $Endpoint.TrimEnd('/')
$script:isEmulator = $Endpoint -match '127\.0\.0\.1|localhost'
$script:results = [System.Collections.Generic.List[object]]::new()

# ---------------------------------------------------------------------------------------------
# REST helper: SharedKeyLite podpis (Table service: StringToSign = x-ms-date + "\n" + CanonicalizedResource)
# ---------------------------------------------------------------------------------------------
function Invoke-Table
{
	param(
		[string] $Method,
		[string] $Path,                  # např. "Tables", "mytable", "mytable(PartitionKey='a',RowKey='b')", '$batch'
		[string] $Query = '',            # např. '$filter=...'
		[string] $Body,
		[hashtable] $Headers = @{},
		[string] $ContentType = 'application/json'
	)

	$date = [DateTime]::UtcNow.ToString('R')
	$uri = "$Endpoint/$Path"
	if ($Query) { $uri += "?$Query" }
	if ($SasToken) { $uri += ($(if ($Query) { '&' } else { '?' })) + $SasToken.TrimStart('?') }

	$allHeaders = @{
		'x-ms-date' = $date
		'x-ms-version' = '2020-12-06'
		'Accept' = 'application/json;odata=minimalmetadata'
		'DataServiceVersion' = '3.0;NetFx'
		'MaxDataServiceVersion' = '3.0;NetFx'
	}
	if (-not $SasToken)
	{
		$uriPath = ([Uri]$uri).AbsolutePath   # u emulátoru obsahuje i segment s názvem účtu, přesně tak to podpis chce
		$canonicalizedResource = "/$AccountName$uriPath"
		$stringToSign = "$date`n$canonicalizedResource"
		$hmac = [System.Security.Cryptography.HMACSHA256]::new([Convert]::FromBase64String($AccountKey))
		$signature = [Convert]::ToBase64String($hmac.ComputeHash([Text.Encoding]::UTF8.GetBytes($stringToSign)))
		$allHeaders['Authorization'] = "SharedKeyLite ${AccountName}:$signature"
	}
	foreach ($key in $Headers.Keys) { $allHeaders[$key] = $Headers[$key] }

	$splat = @{ Method = $Method; Uri = $uri; Headers = $allHeaders; SkipHttpErrorCheck = $true; UseBasicParsing = $true }
	if ($null -ne $Body) { $splat.Body = [Text.Encoding]::UTF8.GetBytes($Body); $splat.ContentType = $ContentType }

	$response = Invoke-WebRequest @splat
	$content = if ($response.Content -is [byte[]]) { [Text.Encoding]::UTF8.GetString($response.Content) } else { [string]$response.Content }
	$json = $null
	if ($content -and $content.TrimStart().StartsWith('{')) { try { $json = $content | ConvertFrom-Json -AsHashtable } catch { } }

	[pscustomobject]@{
		Status = [int]$response.StatusCode
		Headers = $response.Headers
		Raw = $content
		Json = $json
		ErrorCode = if ($json -and $json.ContainsKey('odata.error')) { $json['odata.error'].code } else { $null }
	}
}

function Insert-Entity([hashtable] $Entity)
{
	Invoke-Table -Method POST -Path $TableName -Body ($Entity | ConvertTo-Json -Depth 10 -Compress)
}

function Query-Entities([string] $Query)
{
	$r = Invoke-Table -Method GET -Path "$TableName()" -Query $Query
	if ($r.Status -eq 200) { $r | Add-Member -NotePropertyName Rows -NotePropertyValue @($r.Json['value']) }
	$r
}

function Report
{
	param([string] $Case, [string] $Expected, [string] $Actual, [bool] $Ok, [string] $Note = '', [switch] $EmulatorUnreliable)
	$verdict = if ($Ok) { 'FUNGUJE' } else { 'NEFUNGUJE' }
	$color = if ($Ok) { 'Green' } else { 'Red' }
	if ($EmulatorUnreliable -and $script:isEmulator)
	{
		$verdict = 'NEPRŮKAZNÉ'
		$color = 'Yellow'
		$Note = ('Azurite se zde chová jinak než reálná služba; platí sloupec "očekávání". ' + $Note).Trim()
	}
	Write-Host ("[{0,-10}] {1}" -f $verdict, $Case) -ForegroundColor $color
	Write-Host ("            očekávání: {0}" -f $Expected) -ForegroundColor DarkGray
	Write-Host ("            skutečnost: {0}" -f $Actual)
	if ($Note) { Write-Host ("            pozn.: {0}" -f $Note) -ForegroundColor Yellow }
	$script:results.Add([pscustomobject]@{ Případ = $Case; Verdikt = $verdict; Skutečnost = $Actual })
}

function Describe([object] $r)
{
	$msg = if ($r.ErrorCode) { " $($r.ErrorCode)" } else { '' }
	"HTTP $($r.Status)$msg"
}

# ---------------------------------------------------------------------------------------------
Write-Host "Cíl: $Endpoint  tabulka: $TableName  emulátor: $script:isEmulator" -ForegroundColor Cyan
Write-Host ''

# 0. Tabulka
$r = Invoke-Table -Method POST -Path 'Tables' -Body (@{ TableName = $TableName } | ConvertTo-Json -Compress)
if ($r.Status -notin 201, 204) { throw "Nepodařilo se založit tabulku: $($r.Raw)" }

try
{
	# 1. Plochá entita s typovanými vlastnostmi
	$r = Insert-Entity @{
		PartitionKey = 'osoba'; RowKey = '11111111-1111-1111-1111-111111111111'
		Jmeno = 'Jan'; Prijmeni = 'Čapek'; Aktivni = $true; PocetUcasti = 12
		Vaha = 81.5
		'Vytvoreno@odata.type' = 'Edm.DateTime'; Vytvoreno = '2026-10-16T18:00:00Z'
		'VelkeCislo@odata.type' = 'Edm.Int64'; VelkeCislo = '9007199254740993'
		'Identifikator@odata.type' = 'Edm.Guid'; Identifikator = '22222222-2222-2222-2222-222222222222'
	}
	Report 'Plochá entita: string, bool, int32, double, DateTime, Int64, Guid' 'HTTP 201' (Describe $r) ($r.Status -eq 201)

	# 2. Vnořený objekt
	$r = Insert-Entity @{ PartitionKey = 'termin'; RowKey = '2026-10-20-nested'; Datum = '2026-10-20'; Misto = @{ Nazev = 'Hala'; Kapacita = 12 } }
	Report 'Vnořený objekt jako hodnota vlastnosti' 'HTTP 400 (Edm typy jsou jen skalární)' (Describe $r) ($r.Status -eq 201)

	# 3. Pole
	$r = Insert-Entity @{ PartitionKey = 'termin'; RowKey = '2026-10-20-array'; Datum = '2026-10-20'; Prihlasky = @('a', 'b', 'c') }
	Report 'Pole jako hodnota vlastnosti' 'HTTP 400' (Describe $r) ($r.Status -eq 201)

	# 4. JSON jako string (to, co zbývá pro vnořená data)
	$prihlasky = @(
		@{ OsobaId = '11111111-1111-1111-1111-111111111111'; DatumPrihlaseni = '2026-10-16T18:00:00' }
		@{ OsobaId = '33333333-3333-3333-3333-333333333333'; DatumPrihlaseni = '2026-10-16T19:00:00'; Deleted = '2026-10-17T08:00:00' }
	)
	$r = Insert-Entity @{ PartitionKey = '2026-10-20'; RowKey = '2026-10-20'; Datum = '2026-10-20'; PrihlaskyJson = ($prihlasky | ConvertTo-Json -Compress) }
	$readBack = Invoke-Table -Method GET -Path "$TableName(PartitionKey='2026-10-20',RowKey='2026-10-20')"
	$deserialized = @($readBack.Json['PrihlaskyJson'] | ConvertFrom-Json)
	Report 'Vnořená data jako JSON string, deserializace na klientovi' 'HTTP 201, po přečtení 2 přihlášky' ("$(Describe $r), přečteno $($deserialized.Count) přihlášek") ($r.Status -eq 201 -and $deserialized.Count -eq 2)

	# 5. Dotaz do JSON stringu
	$r = Query-Entities ('$filter=' + [Uri]::EscapeDataString("PrihlaskyJson/OsobaId eq '11111111-1111-1111-1111-111111111111'"))
	Report 'Dotaz dovnitř JSON stringu (PrihlaskyJson/OsobaId eq ...)' 'HTTP 400 nebo 0 řádků - služba string neparsuje' ("$(Describe $r)" + $(if ($r.Status -eq 200) { ", řádků: $($r.Rows.Count)" })) ($r.Status -eq 200 -and $r.Rows.Count -gt 0)

	# 5b. Jediná náhrada: substring hledání neexistuje, jen porovnání celého stringu a prefixu (ge/lt)
	$r = Query-Entities ('$filter=' + [Uri]::EscapeDataString("substringof('11111111', PrihlaskyJson)"))
	Report "substringof()/contains() nad stringem" 'HTTP 400 - Table Storage nemá textové funkce' (Describe $r) ($r.Status -eq 200 -and $r.Rows.Count -gt 0)

	# 6. Schéma per řádek
	$r1 = Insert-Entity @{ PartitionKey = 'ruzne'; RowKey = '1'; Barva = 'modrá' }
	$r2 = Insert-Entity @{ PartitionKey = 'ruzne'; RowKey = '2'; Rychlost = 42; Smer = 'sever' }
	Report 'Dva řádky s úplně jinou sadou vlastností v jedné tabulce' 'HTTP 201 + 201' ("$(Describe $r1), $(Describe $r2)") ($r1.Status -eq 201 -and $r2.Status -eq 201)

	# 7. Filtr na vlastnost, která na některých řádcích chybí (soft delete bez hodnoty)
	$null = Insert-Entity @{ PartitionKey = 'sd'; RowKey = 'aktivni'; Nazev = 'A' }
	$null = Insert-Entity @{ PartitionKey = 'sd'; RowKey = 'smazany'; Nazev = 'B'; 'Deleted@odata.type' = 'Edm.DateTime'; Deleted = '2026-10-01T00:00:00Z' }
	$rHas = Query-Entities ('$filter=' + [Uri]::EscapeDataString("PartitionKey eq 'sd' and Deleted ge datetime'2000-01-01T00:00:00Z'"))
	$rNot = Query-Entities ('$filter=' + [Uri]::EscapeDataString("PartitionKey eq 'sd' and not (Deleted ge datetime'2000-01-01T00:00:00Z')"))
	$rAll = Query-Entities ('$filter=' + [Uri]::EscapeDataString("PartitionKey eq 'sd'"))
	Report 'Negace filtru na chybějící vlastnost (náhrada NOT IS_DEFINED)' "`"not (Deleted ge ...)`" vrátí i řádek bez Deleted? Dokumentace: řádek bez vlastnosti je vždy vyřazen" ("Deleted ge: $($rHas.Rows.Count) řádek, not(Deleted ge): $($rNot.Rows.Count) řádek, bez filtru: $($rAll.Rows.Count)") ($rNot.Status -eq 200 -and $rNot.Rows.Count -eq 1 -and $rNot.Rows[0].RowKey -eq 'aktivni') -EmulatorUnreliable "Řádek bez vlastnosti filtrem nikdy neprojde, ani přes negaci - 'nesmazané' se musí filtrovat na klientovi, nebo ukládat IsDeleted=false explicitně."

	# 8. $orderby
	$r = Query-Entities ('$orderby=' + [Uri]::EscapeDataString('Datum desc'))
	Report '$orderby v dotazu' 'HTTP 400 - řazení existuje jen podle PartitionKey, RowKey' (Describe $r) ($r.Status -eq 200) -EmulatorUnreliable

	# 9. Range dotaz nad klíčem + pořadí výsledku
	foreach ($d in '2026-11-03', '2026-10-27', '2026-11-10') { $null = Insert-Entity @{ PartitionKey = $d; RowKey = $d; Datum = $d } }
	$r = Query-Entities ('$filter=' + [Uri]::EscapeDataString("PartitionKey ge '2026-10-25' and PartitionKey lt '2026-11-30'"))
	$keys = @($r.Rows | ForEach-Object { $_.PartitionKey })
	Report 'Range dotaz nad PartitionKey, výsledek seřazený podle klíče' '3 řádky v pořadí 10-27, 11-03, 11-10' ("$(Describe $r): " + ($keys -join ', ')) ($r.Status -eq 200 -and ($keys -join ',') -eq '2026-10-27,2026-11-03,2026-11-10')

	# 10. $top a $select
	$r = Query-Entities ('$top=2&$select=' + [Uri]::EscapeDataString('PartitionKey,Datum'))
	Report '$top a $select (projekce)' 'HTTP 200, 2 řádky, jen vybrané sloupce' ("$(Describe $r), řádků: $($r.Rows.Count), sloupce: " + (($r.Rows[0].Keys | Where-Object { $_ -notmatch 'odata' }) -join ',')) ($r.Status -eq 200 -and $r.Rows.Count -eq 2)

	# 11. Agregace
	$r = Invoke-Table -Method GET -Path "$TableName()" -Query ('$count=true&$filter=' + [Uri]::EscapeDataString("PartitionKey eq 'sd'"))
	Report 'Agregace ($count, COUNT/SUM/GROUP BY)' 'nepodporováno - $count je ignorováno nebo 400' ("$(Describe $r)" + $(if ($r.Json -and $r.Json.ContainsKey('odata.count')) { ', odata.count vrácen' } else { ', žádný count v odpovědi' })) ($r.Json -and $r.Json.ContainsKey('odata.count'))

	# 12. Duplicitní klíč
	$r = Insert-Entity @{ PartitionKey = '2026-10-20'; RowKey = '2026-10-20'; Datum = '2026-10-20' }
	Report 'Insert na existující PartitionKey+RowKey' 'HTTP 409 EntityAlreadyExists (unikátnost zdarma, jako Cosmos id)' (Describe $r) ($r.Status -eq 409)

	# 13. Podmíněný update přes ETag
	$current = Invoke-Table -Method GET -Path "$TableName(PartitionKey='2026-10-20',RowKey='2026-10-20')"
	$etag = $current.Headers['ETag'] | Select-Object -First 1
	$body = @{ PartitionKey = '2026-10-20'; RowKey = '2026-10-20'; Datum = '2026-10-20'; PrihlaskyJson = '[]' } | ConvertTo-Json -Compress
	$rOk = Invoke-Table -Method PUT -Path "$TableName(PartitionKey='2026-10-20',RowKey='2026-10-20')" -Body $body -Headers @{ 'If-Match' = $etag }
	$rStale = Invoke-Table -Method PUT -Path "$TableName(PartitionKey='2026-10-20',RowKey='2026-10-20')" -Body $body -Headers @{ 'If-Match' = $etag }
	Report 'Replace s If-Match ETag (optimistická konkurence)' 'první 204, druhý se starým ETagem 412' ("$(Describe $rOk), potom $(Describe $rStale)") ($rOk.Status -eq 204 -and $rStale.Status -eq 412)

	# 14. Merge vs Replace
	$null = Insert-Entity @{ PartitionKey = 'mr'; RowKey = '1'; A = 'a'; B = 'b' }
	$null = Invoke-Table -Method MERGE -Path "$TableName(PartitionKey='mr',RowKey='1')" -Body (@{ A = 'a2' } | ConvertTo-Json -Compress) -Headers @{ 'If-Match' = '*' }
	$afterMerge = Invoke-Table -Method GET -Path "$TableName(PartitionKey='mr',RowKey='1')"
	$null = Invoke-Table -Method PUT -Path "$TableName(PartitionKey='mr',RowKey='1')" -Body (@{ A = 'a3' } | ConvertTo-Json -Compress) -Headers @{ 'If-Match' = '*' }
	$afterReplace = Invoke-Table -Method GET -Path "$TableName(PartitionKey='mr',RowKey='1')"
	Report 'MERGE zachová nezmíněné vlastnosti, PUT (Replace) je smaže' 'po MERGE B zůstane, po PUT B zmizí' ("po MERGE B=$($afterMerge.Json['B']); po PUT má B: $($afterReplace.Json.ContainsKey('B'))") ($afterMerge.Json['B'] -eq 'b' -and -not $afterReplace.Json.ContainsKey('B')) "Null se nikdy neuloží; 'smazání hodnoty' = Replace bez té vlastnosti."

	# 15. DateTime bez zóny se vrátí jako UTC
	$null = Insert-Entity @{ PartitionKey = 'dt'; RowKey = '1'; 'Kdy@odata.type' = 'Edm.DateTime'; Kdy = '2026-10-16T18:00:00' }
	$r = Invoke-Table -Method GET -Path "$TableName(PartitionKey='dt',RowKey='1')"
	Report 'Edm.DateTime zapsaný bez zóny' 'vrátí se se Z (služba vše drží jako UTC)' ("uloženo 2026-10-16T18:00:00, vráceno " + [regex]::Match($r.Raw, '"Kdy":"([^"]*)"').Groups[1].Value) ($r.Status -eq 200) "Pražský wall-clock čas je bezpečnější ukládat jako string, jinak klient dostane Kind=Utc."

	# 16. Batch (Entity Group Transaction) v jedné partition vs. napříč
	function Invoke-Batch([object[]] $Entities)
	{
		$batch = 'batch_' + [Guid]::NewGuid()
		$changeset = 'changeset_' + [Guid]::NewGuid()
		$sb = [Text.StringBuilder]::new()
		[void]$sb.Append("--$batch`r`nContent-Type: multipart/mixed; boundary=$changeset`r`n`r`n")
		foreach ($e in $Entities)
		{
			[void]$sb.Append("--$changeset`r`nContent-Type: application/http`r`nContent-Transfer-Encoding: binary`r`n`r`n")
			[void]$sb.Append("POST $Endpoint/$TableName HTTP/1.1`r`nContent-Type: application/json`r`nAccept: application/json;odata=minimalmetadata`r`n`r`n")
			[void]$sb.Append(($e | ConvertTo-Json -Compress) + "`r`n")
		}
		[void]$sb.Append("--$changeset--`r`n--$batch--`r`n")
		$r = Invoke-Table -Method POST -Path '$batch' -Body $sb.ToString() -ContentType "multipart/mixed; boundary=$batch"
		$codes = [regex]::Matches($r.Raw, 'HTTP/1\.1 (\d{3})') | ForEach-Object { $_.Groups[1].Value }
		[pscustomobject]@{ Status = $r.Status; Codes = @($codes); Raw = $r.Raw }
	}
	$same = Invoke-Batch @(@{ PartitionKey = 'b1'; RowKey = '1'; X = 1 }, @{ PartitionKey = 'b1'; RowKey = '2'; X = 2 })
	$cross = Invoke-Batch @(@{ PartitionKey = 'b2'; RowKey = '1'; X = 1 }, @{ PartitionKey = 'b3'; RowKey = '1'; X = 2 })
	Report 'Transakce (batch) nad více entitami v jedné partition' 'HTTP 202, uvnitř 201 + 201' ("HTTP $($same.Status), uvnitř: " + ($same.Codes -join ', ')) ($same.Status -eq 202 -and ($same.Codes -join ',') -eq '201,201')
	Report 'Transakce (batch) napříč partitions' 'odmítnuto (400 uvnitř batche)' ("HTTP $($cross.Status), uvnitř: " + ($cross.Codes -join ', ')) ($cross.Status -eq 202 -and ($cross.Codes -join ',') -eq '201,201') -EmulatorUnreliable

	# 17. Limity (Azurite je nehlídá)
	$r = Insert-Entity @{ PartitionKey = 'lim'; RowKey = 'big'; Data = ('x' * 70000) }
	Report 'String vlastnost přes 64 KB (70 000 znaků)' 'HTTP 400 PropertyValueTooLarge' (Describe $r) ($r.Status -eq 201)
	$many = @{ PartitionKey = 'lim'; RowKey = 'many' }
	1..260 | ForEach-Object { $many["P$_"] = $_ }
	$r = Insert-Entity $many
	Report 'Entita s 260 vlastnostmi (limit 255 včetně klíčů)' 'HTTP 400 TooManyProperties' (Describe $r) ($r.Status -eq 201) -EmulatorUnreliable
	$r = Insert-Entity @{ PartitionKey = 'lim'; RowKey = 'dotted'; 'Misto.Nazev' = 'Hala' }
	Report "Tečka v názvu vlastnosti ('Misto.Nazev' jako náhrada vnoření)" 'HTTP 400 PropertyNameInvalid' (Describe $r) ($r.Status -eq 201) -EmulatorUnreliable
}
finally
{
	$null = Invoke-Table -Method DELETE -Path "Tables('$TableName')"
}

Write-Host ''
Write-Host 'Souhrn:' -ForegroundColor Cyan
$width = ($script:results | ForEach-Object { $_.Případ.Length } | Measure-Object -Maximum).Maximum
foreach ($row in $script:results)
{
	$color = switch ($row.Verdikt) { 'FUNGUJE' { 'Green' } 'NEFUNGUJE' { 'Red' } default { 'Yellow' } }
	Write-Host ("  {0,-10}  " -f $row.Verdikt) -ForegroundColor $color -NoNewline
	Write-Host ("{0,-$width}  {1}" -f $row.Případ, $row.Skutečnost)
}
if ($script:isEmulator) { Write-Host 'Případy označené NEPRŮKAZNÉ spusťte proti reálnému účtu (-AccountName/-AccountKey nebo -SasToken).' -ForegroundColor Yellow }
