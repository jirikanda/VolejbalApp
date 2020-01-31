using KandaEu.Volejbal.Contracts.Osoby.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Facades.Osoby
{
	public interface IOsobaFacade
	{
		void VlozOsobu(OsobaInputDto osobaInputDto);
		void SmazNeaktivniOsobu(int osobaId);
		void AktivujNeaktivniOsobu(int osobaId);
		OsobaListDto GetNeaktivniOsoby();
	}
}
