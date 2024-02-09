using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using Microsoft.AspNetCore.Mvc;

namespace KandaEu.Volejbal.WebAPI.Controllers;

public class NastenkaController
{
	private readonly INastenkaFacade nastenkaFacade;

	public NastenkaController(INastenkaFacade nastenkaFacade)
	{
		this.nastenkaFacade = nastenkaFacade;
	}

	[HttpGet("/api/nastenka")]
	public async Task<VzkazListDto> GetVzkazyAsync(CancellationToken cancellationToken) => await nastenkaFacade.GetVzkazyAsync(cancellationToken);

	[HttpPost("/api/nastenka")]
	public async Task VlozVzkazAsync(VzkazInputDto vzkaz, CancellationToken cancellationToken) => await nastenkaFacade.VlozVzkazAsync(vzkaz, cancellationToken);
}

