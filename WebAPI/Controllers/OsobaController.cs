using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Facades.Osoby;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.WebAPI.Controllers;

public class OsobaController
{
	private readonly IOsobaFacade osobaFacade;

	public OsobaController(IOsobaFacade osobaFacade)
	{
		this.osobaFacade = osobaFacade;
	}

	[HttpPost("api/osoby")]
	public async Task VlozOsobu(OsobaInputDto osobaInputDto) => await osobaFacade.VlozOsobu(osobaInputDto);

	[HttpGet("api/osoby/aktivni")]
	public async Task<OsobaListDto> GetAktivniOsoby() => await osobaFacade.GetAktivniOsoby();

	[HttpGet("api/osoby/neaktivni")]
	public async Task<OsobaListDto> GetNeaktivniOsoby() => await osobaFacade.GetNeaktivniOsoby();

	[HttpDelete("api/osoby/neaktivni/{osobaId}")]
	public async Task SmazNeaktivniOsobu(int osobaId) => await osobaFacade.SmazNeaktivniOsobu(osobaId);

	[HttpPost("api/osoby/neaktivni/{osobaId}/aktivovat")]
	public async Task AktivujNeaktivniOsobu(int osobaId) => await osobaFacade.AktivujNeaktivniOsobu(osobaId);
}
