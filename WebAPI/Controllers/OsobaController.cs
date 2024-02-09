using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using Microsoft.AspNetCore.Mvc;

namespace KandaEu.Volejbal.WebAPI.Controllers;

public class OsobaController
{
	private readonly IOsobaFacade osobaFacade;

	public OsobaController(IOsobaFacade osobaFacade)
	{
		this.osobaFacade = osobaFacade;
	}

	[HttpPost("api/osoby")]
	public async Task VlozOsobuAsync(OsobaInputDto osobaInputDto, CancellationToken cancellationToken) => await osobaFacade.VlozOsobuAsync(osobaInputDto, cancellationToken);

	[HttpGet("api/osoby/aktivni")]
	public async Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken) => await osobaFacade.GetAktivniOsobyAsync(cancellationToken);

	[HttpGet("api/osoby/neaktivni")]
	public async Task<OsobaListDto> GetNeaktivniOsobyAsync(CancellationToken cancellationToken) => await osobaFacade.GetNeaktivniOsobyAsync(cancellationToken);

	[HttpDelete("api/osoby/neaktivni/{osobaId}")]
	public async Task SmazNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken) => await osobaFacade.SmazNeaktivniOsobuAsync(osobaId, cancellationToken);

	[HttpPost("api/osoby/neaktivni/{osobaId}/aktivovat")]
	public async Task AktivujNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken) => await osobaFacade.AktivujNeaktivniOsobuAsync(osobaId, cancellationToken);
}
