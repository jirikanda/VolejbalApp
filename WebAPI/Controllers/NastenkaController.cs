using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using KandaEu.Volejbal.Facades.Vzkazy;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.WebAPI.Controllers
{
	public class NastenkaController
	{
		private readonly INastenkaFacade nastenkaFacade;

		public NastenkaController(INastenkaFacade nastenkaFacade)
		{
			this.nastenkaFacade = nastenkaFacade;
		}

		[HttpGet("/api/nastenka")]
		public VzkazListDto GetVzkazy()
		{
			return nastenkaFacade.GetVzkazy();
		}

		[HttpPost("/api/nastenka")]
		public void VlozVzkaz(VzkazInputDto vzkaz)
		{
			nastenkaFacade.VlozVzkaz(vzkaz);
		}
	}

}

