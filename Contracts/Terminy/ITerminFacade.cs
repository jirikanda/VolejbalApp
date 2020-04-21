using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Terminy
{
	[ServiceContract]
	public interface ITerminFacade
	{
		Task<GetTerminyResponse> GetTerminy();
		Task<GetDetailTerminuResult> GetDetailTerminu(GetDetailTerminuRequest getDetailTerminuRequest);
	}
}