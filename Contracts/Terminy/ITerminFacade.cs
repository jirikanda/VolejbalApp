using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System.Collections.Generic;

namespace KandaEu.Volejbal.Contracts.Terminy
{
	public interface ITerminFacade
	{
		TerminListDto GetTerminy();
		TerminDetailDto GetDetailTerminu(int terminId);
	}
}