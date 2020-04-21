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
		Task VlozOsobu(VlozOsobuRequest vlozOsobuRequest);
		Task SmazNeaktivniOsobu(SmazNeaktivniOsobuRequest smazNeaktivniOsobuRequest);
		Task AktivujNeaktivniOsobu(AktivujNeaktivniOsobuRequest aktivujNeaktivniOsobuRequest);
		Task<GetOsobyResponse> GetNeaktivniOsoby();
		Task<GetOsobyResponse> GetAktivniOsoby();
	}
}
