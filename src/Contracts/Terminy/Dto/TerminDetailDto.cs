using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto;

public class TerminDetailDto
{
	public List<PrihlasenaOsobaDto> Prihlaseni { get; set; }
	public List<NeprihlasenaOsobaDto> Neprihlaseni { get; set; }
}
