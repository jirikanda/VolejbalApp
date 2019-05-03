using KandaEu.Volejbal.Facades.Prihlasky;
using KandaEu.Volejbal.Facades.Prihlasky.Dto;
using KandaEu.Volejbal.Facades.Terminy;
using KandaEu.Volejbal.Facades.Terminy.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypeLite;

namespace Havit.VolejbalApp.WebAPI.Controllers
{
	public class TerminController
	{
		private readonly ITerminFacade terminFacade;
		private readonly IPrihlaskaFacade prihlaskaFacade;

		public TerminController(ITerminFacade terminFacade, IPrihlaskaFacade prihlaskaFacade)
		{
			this.terminFacade = terminFacade;
			this.prihlaskaFacade = prihlaskaFacade;
		}

		[HttpGet("/api/terminy")]
		public TerminListDto GetTerminy()
		{
			return terminFacade.GetTerminy();
		}

		[HttpGet("/api/terminy/{terminId}")]
		public TerminDetailDto GetDetailTerminu(int terminId)
		{
			return terminFacade.GetDetailTerminu(terminId);
		}

		[HttpPost("/api/terminy/{terminId}/prihlasit")]
		public void Prihlasit(int terminId, PrihlaskaOdhlaskaDto prihlaskaDto)
		{
			prihlaskaFacade.Prihlasit(terminId, prihlaskaDto);
		}

		[HttpPost("/api/terminy/{terminId}/odhlasit")]
		public void Odhlasit(int terminId, PrihlaskaOdhlaskaDto odhlaskaDto)
		{
			prihlaskaFacade.Prihlasit(terminId, odhlaskaDto);
		}
	}
}
