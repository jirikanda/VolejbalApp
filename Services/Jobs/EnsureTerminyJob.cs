using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

namespace KandaEu.Volejbal.Services.Jobs;

[Service]
public class EnsureTerminyJob(IEnsureTerminyService _ensureTerminyService) : IEnsureTerminyJob
{
	public async Task ExecuteAsync(CancellationToken cancellationToken)
	{
		await _ensureTerminyService.EnsureTerminyAsync(cancellationToken);
	}
}
