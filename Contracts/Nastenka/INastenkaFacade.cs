using KandaEu.Volejbal.Contracts.Nastenka.Dto;

namespace KandaEu.Volejbal.Contracts.Nastenka;

public interface INastenkaFacade
{
	Task<VzkazListDto> GetVzkazy();

	Task VlozVzkaz(VzkazInputDto vzkaz);
}
