using KandaEu.Volejbal.Services.DeaktivaceOsob;

namespace KandaEu.Volejbal.Services.Jobs;

[Service]
public class DeaktivaceOsobJob(IDeaktivaceOsobService _deaktivaceOsobService) : IDeaktivaceOsobJob
{
	public async Task ExecuteAsync(CancellationToken cancellationToken = default)
	{
		await _deaktivaceOsobService.DeaktivujOsobyAsync(cancellationToken);
	}
}
