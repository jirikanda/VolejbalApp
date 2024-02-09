using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using Microsoft.AspNetCore.Mvc;

namespace KandaEu.Volejbal.WebAPI.Controllers;

public class OsobaController(IOsobaFacade _osobaFacade)
{
	[HttpPost("api/osoby")]
	public async Task VlozOsobuAsync(OsobaInputDto osobaInputDto, CancellationToken cancellationToken) => await _osobaFacade.VlozOsobuAsync(osobaInputDto, cancellationToken);

	[HttpGet("api/osoby/aktivni")]
	public async Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken) => await _osobaFacade.GetAktivniOsobyAsync(cancellationToken);

	[HttpGet("api/osoby/neaktivni")]
	public async Task<OsobaListDto> GetNeaktivniOsobyAsync(CancellationToken cancellationToken) => await _osobaFacade.GetNeaktivniOsobyAsync(cancellationToken);

	[HttpDelete("api/osoby/neaktivni/{osobaId}")]
	public async Task SmazNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken) => await _osobaFacade.SmazNeaktivniOsobuAsync(osobaId, cancellationToken);

	[HttpPost("api/osoby/neaktivni/{osobaId}/aktivovat")]
	public async Task AktivujNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken) => await _osobaFacade.AktivujNeaktivniOsobuAsync(osobaId, cancellationToken);
}
