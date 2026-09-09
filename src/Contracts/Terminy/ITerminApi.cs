using KandaEu.Volejbal.Contracts.Api;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using Refit;

namespace KandaEu.Volejbal.Contracts.Terminy;

public interface ITerminApi
{
	[Get("/" + ApiRoutes.Terminy)]
	Task<TerminListDto> GetTerminyAsync(CancellationToken cancellationToken = default);

	[Get("/" + ApiRoutes.Termin)]
	Task<TerminDetailDto> GetDetailTerminuAsync(int terminId, CancellationToken cancellationToken = default);
}
