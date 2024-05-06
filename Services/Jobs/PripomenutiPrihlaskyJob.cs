
using KandaEu.Volejbal.Services.Terminy.PripominkyPrihlaseni;

namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

[Service]
public class PripomenutiPrihlaskyJob(IPripomenutiPrihlaskyService _pripomenutiPrihlaskyService) : IPripomenutiPrihlaskyJob
{
	public async Task ExecuteAsync(CancellationToken cancellationToken = default)
	{
		await _pripomenutiPrihlaskyService.SendPripominkyAsync(cancellationToken);
	}
}
