using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Facades.Vzkazy
{
	public interface INastenkaFacade
	{
		VzkazListDto GetVzkazy();

		void VlozVzkaz(VzkazInputDto vzkaz);
	}
}
