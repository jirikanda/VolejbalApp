using KandaEu.Volejbal.Contracts.Osoby.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Osoby
{
	public interface IOsobaFacade
	{
		Task VlozOsobu(OsobaInputDto osobaInputDto);
		Task SmazNeaktivniOsobu(int osobaId);
		Task AktivujNeaktivniOsobu(int osobaId);
		Task<OsobaListDto> GetNeaktivniOsoby();
		Task<OsobaListDto> GetAktivniOsoby();
	}
}
