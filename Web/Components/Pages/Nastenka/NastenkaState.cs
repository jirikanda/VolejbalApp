using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Web.Components.Pages.Nastenka;

public class NastenkaState
{
	public List<VzkazDto> Vzkazy { get; set; }
	public List<OsobaDto> AktivniOsoby { get; set; }
}
