using KandaEu.Volejbal.Contracts.Terminy.Dto;

namespace KandaEu.Volejbal.Contracts.Terminy;

public interface ITerminFacade
{
	Task<TerminListDto> GetTerminyAsync(CancellationToken cancellationToken = default);
	Task<TerminDetailDto> GetDetailTerminuAsync(int terminId, CancellationToken cancellationToken = default);
}
