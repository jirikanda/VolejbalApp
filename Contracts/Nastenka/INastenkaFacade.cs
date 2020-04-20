using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Nastenka
{
	public interface INastenkaFacade
	{
		VzkazListDto GetVzkazy();

		void VlozVzkaz(VzkazInputDto vzkaz);
	}
}
