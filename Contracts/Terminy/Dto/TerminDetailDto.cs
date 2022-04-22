using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto;

public class TerminDetailDto
{
    public List<OsobaDto> Prihlaseni { get; set; }
    public List<OsobaDto> Neprihlaseni { get; set; }
}
