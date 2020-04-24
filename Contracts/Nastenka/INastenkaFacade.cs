using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Contracts.Nastenka
{
	public interface INastenkaFacade
	{
		Task<GetVzkazyResult> GetVzkazy();

		Task VlozVzkaz(VlozVzkazRequest request);
	}
}
