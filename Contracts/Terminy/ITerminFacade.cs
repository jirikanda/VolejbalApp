using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Terminy
{
	public interface ITerminFacade
	{
		Task<GetTerminyResponse> GetTerminy();
		Task<GetDetailTerminuResult> GetDetailTerminu(GetDetailTerminuRequest getDetailTerminuRequest);
	}
}