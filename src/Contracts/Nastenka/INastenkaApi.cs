using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using Refit;

namespace KandaEu.Volejbal.Contracts.Nastenka;

public interface INastenkaApi
{
	[Get("/" + ApiRoutes.Nastenka)]
	Task<VzkazListDto> GetVzkazyAsync(CancellationToken cancellationToken = default);

	[Post("/" + ApiRoutes.Nastenka)]
	Task VlozVzkazAsync([Body] VzkazInputDto vzkaz, CancellationToken cancellationToken = default);
}
