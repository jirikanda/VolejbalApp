using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Facades.Osoby;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.WebAPI.Controllers
{
	public class OsobaController
	{
		private readonly IOsobaFacade osobaFacade;

		public OsobaController(IOsobaFacade osobaFacade)
		{
			this.osobaFacade = osobaFacade;
		}

		[HttpPost("api/osoby")]
		public void VlozOsobu(OsobaInputDto osobaInputDto)
		{
			osobaFacade.VlozOsobu(osobaInputDto);
		}

		[HttpGet("api/osoby/neaktivni")]
		public OsobaListDto GetNeaktivniOsoby()
		{
			return osobaFacade.GetNeaktivniOsoby();
		}

		[HttpDelete("api/osoby/neaktivni/{osobaId}")]
		public void SmazNeaktivniOsobu(int osobaId)
		{
			osobaFacade.SmazNeaktivniOsobu(osobaId);
		}

		[HttpPost("api/osoby/neaktivni/{osobaId}/aktivuj")]
		public void AktivujNeaktivniOsobu(int osobaId)
		{
			osobaFacade.AktivujNeaktivniOsobu(osobaId);
		}

	}
}
