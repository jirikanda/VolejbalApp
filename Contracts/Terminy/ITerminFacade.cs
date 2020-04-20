using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Terminy
{
	public interface ITerminFacade
	{
		Task<TerminListDto> GetTerminy();
		Task<TerminDetailDto> GetDetailTerminu(int terminId);
	}
}