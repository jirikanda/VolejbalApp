using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Web.Components.Pages.Prihlasovani;

public class PrihlasovaniState
{
	public int? AktualniTerminId { get; set; }

	public List<PrihlasenaOsobaDto> Prihlaseni { get; set; }
	public List<NeprihlasenaOsobaDto> Neprihlaseni { get; set; }

}
