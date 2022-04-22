using KandaEu.Volejbal.Contracts.Terminy.Dto;

namespace KandaEu.Volejbal.Contracts.Terminy;

public interface ITerminFacade
{
    Task<TerminListDto> GetTerminy();
    Task<TerminDetailDto> GetDetailTerminu(int terminId);
}
