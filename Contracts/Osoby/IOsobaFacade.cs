using KandaEu.Volejbal.Contracts.Osoby.Dto;
using System;
using System.Collections.Generic;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Osoby
{
	[ServiceContract]
	public interface IOsobaFacade
	{
		Task VlozOsobu(OsobaInputDto osobaInputDto);
		Task SmazNeaktivniOsobu(int osobaId); // TODO GRPC: Request
		Task AktivujNeaktivniOsobu(int osobaId);// TODO GRPC: Request
		Task<OsobaListDto> GetNeaktivniOsoby();
		Task<OsobaListDto> GetAktivniOsoby();
	}
}
