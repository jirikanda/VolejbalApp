using System.Threading;
using KandaEu.Volejbal.Contracts.Prihlasky;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using Microsoft.AspNetCore.Mvc;

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
	public async Task<TerminListDto> GetTerminyAsync(CancellationToken cancellationToken) => await terminFacade.GetTerminyAsync(cancellationToken);

	[HttpGet("/api/terminy/{terminId}")]
	public async Task<TerminDetailDto> GetDetailTerminuAsync(int terminId, CancellationToken cancellationToken) => await terminFacade.GetDetailTerminuAsync(terminId, cancellationToken);

	[HttpPost("/api/terminy/{terminId}/osoby/{osobaId}/prihlasit")]
	public async Task PrihlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken) => await prihlaskaFacade.PrihlasitAsync(terminId, osobaId, cancellationToken);

	[HttpPost("/api/terminy/{terminId}/osoby/{osobaId}/odhlasit")]
	public async Task OdhlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken) => await prihlaskaFacade.OdhlasitAsync(terminId, osobaId, cancellationToken);
}
