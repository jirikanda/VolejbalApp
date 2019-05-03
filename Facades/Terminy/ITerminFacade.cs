using KandaEu.Volejbal.Facades.Terminy.Dto;
using System.Collections.Generic;

namespace KandaEu.Volejbal.Facades.Terminy
{
	public interface ITerminFacade
	{
		TerminListDto GetTerminy();
		TerminDetailDto GetDetailTerminu(int terminId);
	}
}