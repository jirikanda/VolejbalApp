using System.ComponentModel;
using System.Globalization;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Unicode;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using ModelContextProtocol.Server;

namespace KandaEu.Volejbal.Api.Mcp;

/// <summary>
/// Nástroje MCP serveru (Model Context Protocol) - přihlašování na termíny z AI asistenta.
/// </summary>
/// <remarks>
/// Nástroje jen volají tytéž fasády jako HTTP triggery - pravidla (neaktivní hráč, termín v minulosti,
/// souběh přes ETag) jsou tedy stejná jako ve webu. Výjimky fasád převádí na chybový výsledek nástroje
/// <see cref="McpToolErrorFilter" />. Instance vzniká pro každé volání nástroje z DI scope dané invokace
/// funkce (viz <see cref="McpRequestHandler" />), fasády jsou tedy scoped jako u HTTP triggerů.
/// </remarks>
[McpServerToolType]
public class VolejbalMcpTools(
	ITerminApi _terminFacade,
	IOsobaApi _osobaFacade,
	IPrihlaskaApi _prihlaskaFacade)
{
	private const string DatumDescription = "Datum termínu ve tvaru yyyy-MM-dd (např. 2026-01-13), viz nástroj seznam_terminu.";
	private const string OsobaIdDescription = "Id hráče z nástroje seznam_hracu.";

	/// <summary>
	/// Výsledky se modelu předávají jako JSON text. Čeština zůstává nezakódovaná - escapované č by
	/// model musel dekódovat a stálo by víc tokenů. Text nikdy nekončí v HTML, takže to nevadí.
	/// </summary>
	private static readonly JsonSerializerOptions s_jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
	{
		Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
	};

	private static readonly CultureInfo s_czechCulture = CultureInfo.GetCultureInfo("cs-CZ");

	[McpServerTool(Name = "seznam_hracu", ReadOnly = true, OpenWorld = false)]
	[Description("Vrátí seznam hráčů (id, příjmení a jméno, příznak aktivní) seřazený podle příjmení. Na termín lze přihlásit jen aktivního hráče. Id hráče je potřeba pro nástroje prihlasit a odhlasit.")]
	public async Task<string> SeznamHracuAsync(CancellationToken cancellationToken)
	{
		OsobaListDto osoby = await _osobaFacade.GetOsobyAsync(cancellationToken);
		return JsonSerializer.Serialize(osoby, s_jsonSerializerOptions);
	}

	// ReadOnly záměrně, i když GetTerminyAsync umí dozaložit chybějící budoucí termíny (EnsureTerminyService):
	// je to implementační náhrada časového triggeru, z pohledu volajícího se čtením nic nemění.
	[McpServerTool(Name = "seznam_terminu", ReadOnly = true, OpenWorld = false)]
	[Description("Vrátí nadcházející termíny hraní a ke každému, kdo je přihlášený, kdo se omluvil (odhlásil) a kdo se dosud nevyjádřil.")]
	public async Task<string> SeznamTerminuAsync(CancellationToken cancellationToken)
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
				DenVTydnu = termin.Datum.ToString("dddd", s_czechCulture),
				Prihlaseni = detail.Prihlaseni.Select(prihlaseny => ToMcpOsobaDto(prihlaseny.Osoba)).ToList(),
				Omluveni = detail.Neprihlaseni.Where(neprihlaseny => neprihlaseny.IsOdhlaseny).Select(neprihlaseny => ToMcpOsobaDto(neprihlaseny.Osoba)).ToList(),
				Nerozhodnuti = detail.Neprihlaseni.Where(neprihlaseny => !neprihlaseny.IsOdhlaseny).Select(neprihlaseny => ToMcpOsobaDto(neprihlaseny.Osoba)).ToList()
			});
		}

		return JsonSerializer.Serialize(new McpTerminListDto { Terminy = result }, s_jsonSerializerOptions);
	}

	// Destructive = false záměrně, i když podle specifikace MCP "non-destructive" znamená jen přidávající změny:
	// přihlášení a odhlášení jen přepínají stav, který jde kdykoli vrátit, a o potvrzení každé změny nestojíme.
	[McpServerTool(Name = "prihlasit", Destructive = false, Idempotent = true, OpenWorld = false)]
	[Description("Přihlásí hráče na termín (hráč přijde). Opakované přihlášení nic nezmění. Přihlásit lze jen aktivního hráče a jen na termín, který ještě neproběhl.")]
	public async Task<string> PrihlasitAsync(
		[Description(DatumDescription)] string datum,
		[Description(OsobaIdDescription)] string osobaId,
		CancellationToken cancellationToken)
	{
		// Id termínu je jeho datum ve tvaru yyyy-MM-dd (Termin.GetId), takže datum od modelu se předává
		// rovnou. Jiný tvar repozitář nenajde a fasáda odpoví "Termín nebyl nalezen.".
		await _prihlaskaFacade.PrihlasitAsync(datum, osobaId, cancellationToken);
		return $"Hráč {osobaId} je přihlášený na termín {datum}.";
	}

	// Destructive = false záměrně, viz prihlasit.
	[McpServerTool(Name = "odhlasit", Destructive = false, Idempotent = true, OpenWorld = false)]
	[Description("Odhlásí hráče z termínu (hráč nepřijde). Slouží i k omluvení předem - k vyjádření neúčasti hráče, který přihlášený nebyl. Opakované odhlášení nic nezmění.")]
	public async Task<string> OdhlasitAsync(
		[Description(DatumDescription)] string datum,
		[Description(OsobaIdDescription)] string osobaId,
		CancellationToken cancellationToken)
	{
		await _prihlaskaFacade.OdhlasitAsync(datum, osobaId, cancellationToken);
		return $"Hráč {osobaId} je z termínu {datum} odhlášený (omluvený).";
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
