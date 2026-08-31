using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using Microsoft.AspNetCore.Mvc;

namespace KandaEu.Volejbal.Web.Controllers;

public class OsobaController(IOsobaFacade _osobaFacade)
{
	[HttpPost("api/osoby")]
	public async Task VlozOsobuAsync(OsobaInputDto osobaInputDto, CancellationToken cancellationToken) => await _osobaFacade.VlozOsobuAsync(osobaInputDto, cancellationToken);

	[HttpGet("api/osoby")]
	public async Task<OsobaListDto> GetOsobyAsync(CancellationToken cancellationToken) => await _osobaFacade.GetOsobyAsync(cancellationToken);

	[HttpGet("api/osoby/aktivni")]
	public async Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken) => await _osobaFacade.GetAktivniOsobyAsync(cancellationToken);

	[HttpDelete("api/osoby/{osobaId}")]
	public async Task SmazOsobuAsync(int osobaId, CancellationToken cancellationToken) => await _osobaFacade.SmazOsobuAsync(osobaId, cancellationToken);

	[HttpPost("api/osoby/{osobaId}/aktivovat")]
	public async Task AktivujOsobuAsync(int osobaId, CancellationToken cancellationToken) => await _osobaFacade.AktivujOsobuAsync(osobaId, cancellationToken);

	[HttpPost("api/osoby/{osobaId}/deaktivovat")]
	public async Task DeaktivujOsobuAsync(int osobaId, CancellationToken cancellationToken) => await _osobaFacade.DeaktivujOsobuAsync(osobaId, cancellationToken);
}
