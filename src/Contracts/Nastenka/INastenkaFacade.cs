using KandaEu.Volejbal.Contracts.Nastenka.Dto;

namespace KandaEu.Volejbal.Contracts.Nastenka;

public interface INastenkaFacade
{
	Task<VzkazListDto> GetVzkazyAsync(CancellationToken cancellationToken = default);
	Task VlozVzkazAsync(VzkazInputDto vzkaz, CancellationToken cancellationToken = default);
}
