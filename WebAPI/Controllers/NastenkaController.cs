using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using Microsoft.AspNetCore.Mvc;

namespace KandaEu.Volejbal.WebAPI.Controllers;

public class NastenkaController(INastenkaFacade _nastenkaFacade)
{
	[HttpGet("/api/nastenka")]
	public async Task<VzkazListDto> GetVzkazyAsync(CancellationToken cancellationToken) => await _nastenkaFacade.GetVzkazyAsync(cancellationToken);

	[HttpPost("/api/nastenka")]
	public async Task VlozVzkazAsync(VzkazInputDto vzkaz, CancellationToken cancellationToken) => await _nastenkaFacade.VlozVzkazAsync(vzkaz, cancellationToken);
}

