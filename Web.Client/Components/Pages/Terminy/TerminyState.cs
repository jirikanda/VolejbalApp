using KandaEu.Volejbal.Contracts.Terminy.Dto;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Terminy;

public class TerminyState
{
	public List<TerminDto> Terminy { get; set; }
	public int? CurrentTerminId { get; set; }
}
