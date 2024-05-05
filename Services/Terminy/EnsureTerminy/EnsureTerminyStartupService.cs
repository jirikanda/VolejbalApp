using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

public class EnsureTerminyStartupService(
	IServiceProvider _serviceProvider) : IHostedService
{
	public async Task StartAsync(CancellationToken cancellationToken)
	{
		using var scope = _serviceProvider.CreateScope();
		var ensureTerminyService = scope.ServiceProvider.GetRequiredService<IEnsureTerminyService>();
		await ensureTerminyService.EnsureTerminyAsync(cancellationToken);
	}

	public Task StopAsync(CancellationToken cancellationToken)
	{
		return Task.CompletedTask;
	}
}
