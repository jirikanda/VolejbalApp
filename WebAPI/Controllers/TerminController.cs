using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.Facades.Prihlasky;
using KandaEu.Volejbal.Facades.Terminy;
using KandaEu.Volejbal.Facades.Terminy.Dto;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TypeLite;

namespace Havit.VolejbalApp.WebAPI.Controllers;

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
	public async Task<TerminListDto> GetTerminy() => await terminFacade.GetTerminy();

	[HttpGet("/api/terminy/{terminId}")]
	public async Task<TerminDetailDto> GetDetailTerminu(int terminId) => await terminFacade.GetDetailTerminu(terminId);

	[HttpPost("/api/terminy/{terminId}/osoby/{osobaId}/prihlasit")]
	public Task Prihlasit(int terminId, int osobaId) => prihlaskaFacade.Prihlasit(terminId, osobaId);

	[HttpPost("/api/terminy/{terminId}/osoby/{osobaId}/odhlasit")]
	public Task Odhlasit(int terminId, int osobaId) => prihlaskaFacade.Odhlasit(terminId, osobaId);
}
