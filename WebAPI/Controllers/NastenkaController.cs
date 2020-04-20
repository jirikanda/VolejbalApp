using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;
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
		public async Task<VzkazListDto> GetVzkazy() => await nastenkaFacade.GetVzkazy();

		[HttpPost("/api/nastenka")]
		public async Task VlozVzkaz(VzkazInputDto vzkaz) => await nastenkaFacade.VlozVzkaz(vzkaz);
	}
}

