using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using KandaEu.Volejbal.Api.Mcp;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.Model;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Extensions.Mcp;

namespace KandaEu.Volejbal.Api.Functions;

/// <summary>
/// Nástroje MCP serveru (Model Context Protocol) - přihlašování na termíny z AI asistenta.
/// </summary>
/// <remarks>
/// Endpoint MCP serveru vystavuje MCP extension hostu (/runtime/webhooks/mcp), ne HTTP trigger, takže
/// nástroje nejsou součástí REST kontraktu (I*Api) a ApiContractTests se jich netýkají. Nástroje jen
/// volají tytéž fasády jako HTTP triggery - pravidla (neaktivní hráč, termín v minulosti, souběh přes
/// ETag) jsou tedy stejná jako ve webu. Chyby převádí na text výsledku ExceptionHandlingMiddleware.
///
/// Parametry nástrojů jsou záměrně DateTimeOffset a Guid, ne string. MCP extension (1.6.0) při čtení
/// argumentů převádí každý řetězec, který vypadá jako datum, na DateTimeOffset (a GUID na Guid) a do
/// string parametru ho pak vrací přes Convert.ToString - z "2026-01-13" se tak stane
/// "01/13/2026 00:00:00 +00:00" a termín se nenajde. S typovaným parametrem se hodnota předá beze změny
/// a do tvaru id ji převádíme sami. Ve schématu nástroje jsou oba parametry dál typu "string".
/// </remarks>
public class McpFunctions(
	ITerminApi _terminFacade,
	IOsobaApi _osobaFacade,
	IPrihlaskaApi _prihlaskaFacade)
{
	private const string DatumDescription = "Datum termínu ve tvaru yyyy-MM-dd (např. 2026-01-13), viz nástroj seznam_terminu.";
	private const string OsobaIdDescription = "Id hráče (GUID) z nástroje seznam_hracu.";

	/// <summary>
	/// Výsledky se modelu předávají jako JSON text. Čeština zůstává nezakódovaná - escapované č by
	/// model musel dekódovat a stálo by víc tokenů. Text nikdy nekončí v HTML, takže to nevadí.
	/// </summary>
	private static readonly JsonSerializerOptions s_jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
	{
		Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
	};

	[Function(nameof(McpSeznamHracuAsync))]
	public async Task<string> McpSeznamHracuAsync(
		[McpToolTrigger("seznam_hracu", "Vrátí seznam hráčů (id, příjmení a jméno, příznak aktivní) seřazený podle příjmení. Na termín lze přihlásit jen aktivního hráče. Id hráče je potřeba pro nástroje prihlasit a odhlasit.")] ToolInvocationContext context,
		CancellationToken cancellationToken)
	{
		OsobaListDto osoby = await _osobaFacade.GetOsobyAsync(cancellationToken);
		return JsonSerializer.Serialize(osoby, s_jsonSerializerOptions);
	}

	[Function(nameof(McpSeznamTerminuAsync))]
	public async Task<string> McpSeznamTerminuAsync(
		[McpToolTrigger("seznam_terminu", "Vrátí nadcházející termíny hraní a ke každému, kdo je přihlášený, kdo se omluvil (odhlásil) a kdo se dosud nevyjádřil.")] ToolInvocationContext context,
		CancellationToken cancellationToken)
	{
		TerminListDto terminy = await _terminFacade.GetTerminyAsync(cancellationToken);

		// Detail se čte po termínech (budoucí termíny jsou tři) - fasáda pro seznam s detaily nemá metodu
		// a kvůli MCP nemá smysl přidávat endpoint, který web nepotřebuje.
		List<McpTerminDto> result = new List<McpTerminDto>();
		foreach (TerminDto termin in terminy.Terminy)
		{
			TerminDetailDto detail = await _terminFacade.GetDetailTerminuAsync(termin.Id, cancellationToken);
			result.Add(new McpTerminDto
			{
				Datum = termin.Id,
				DenVTydnu = termin.Datum.ToString("dddd", CultureInfo.GetCultureInfo("cs-CZ")),
				Prihlaseni = detail.Prihlaseni.Select(prihlaseny => ToMcpOsobaDto(prihlaseny.Osoba)).ToList(),
				Omluveni = detail.Neprihlaseni.Where(neprihlaseny => neprihlaseny.IsOdhlaseny).Select(neprihlaseny => ToMcpOsobaDto(neprihlaseny.Osoba)).ToList(),
				Nerozhodnuti = detail.Neprihlaseni.Where(neprihlaseny => !neprihlaseny.IsOdhlaseny).Select(neprihlaseny => ToMcpOsobaDto(neprihlaseny.Osoba)).ToList()
			});
		}

		return JsonSerializer.Serialize(new McpTerminListDto { Terminy = result }, s_jsonSerializerOptions);
	}

	[Function(nameof(McpPrihlasitAsync))]
	public async Task<string> McpPrihlasitAsync(
		[McpToolTrigger("prihlasit", "Přihlásí hráče na termín (hráč přijde). Opakované přihlášení nic nezmění. Přihlásit lze jen aktivního hráče a jen na termín, který ještě neproběhl.")] ToolInvocationContext context,
		[McpToolProperty("datum", DatumDescription, isRequired: true)] DateTimeOffset datum,
		[McpToolProperty("osobaId", OsobaIdDescription, isRequired: true)] Guid osobaId,
		CancellationToken cancellationToken)
	{
		string terminId = GetTerminId(datum);
		await _prihlaskaFacade.PrihlasitAsync(terminId, osobaId.ToString(), cancellationToken);
		return $"Hráč {osobaId} je přihlášený na termín {terminId}.";
	}

	[Function(nameof(McpOdhlasitAsync))]
	public async Task<string> McpOdhlasitAsync(
		[McpToolTrigger("odhlasit", "Odhlásí hráče z termínu (hráč nepřijde). Slouží i k omluvení předem - k vyjádření neúčasti hráče, který přihlášený nebyl. Opakované odhlášení nic nezmění.")] ToolInvocationContext context,
		[McpToolProperty("datum", DatumDescription, isRequired: true)] DateTimeOffset datum,
		[McpToolProperty("osobaId", OsobaIdDescription, isRequired: true)] Guid osobaId,
		CancellationToken cancellationToken)
	{
		string terminId = GetTerminId(datum);
		await _prihlaskaFacade.OdhlasitAsync(terminId, osobaId.ToString(), cancellationToken);
		return $"Hráč {osobaId} je z termínu {terminId} odhlášený (omluvený).";
	}

	/// <summary>
	/// Id termínu z data, které poslal model. Bere se datum tak, jak je zapsané (DateTimeOffset.Date),
	/// bez převodu do UTC - "2026-01-13T00:00:00+01:00" je pořád 13. ledna.
	/// </summary>
	private static string GetTerminId(DateTimeOffset datum)
	{
		return Termin.GetId(datum.Date);
	}

	private static McpOsobaDto ToMcpOsobaDto(OsobaDto osoba)
	{
		return new McpOsobaDto
		{
			Id = osoba.Id,
			PrijmeniJmeno = osoba.PrijmeniJmeno
		};
	}
}
