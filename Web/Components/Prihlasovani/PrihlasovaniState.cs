using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Web.Components.Prihlasovani;

public class PrihlasovaniState
{
	public int? AktualniTerminId { get; set; }

	public List<OsobaDto> Prihlaseni { get; set; }
	public List<OsobaDto> Neprihlaseni { get; set; }

}
