using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace KandaEu.Volejbal.Services.DeaktivaceOsob;

public class DeaktivaceOsobBackgroundService : BackgroundService
{
    private readonly IServiceProvider serviceProvider;

    public DeaktivaceOsobBackgroundService(IServiceProvider serviceProvider)
    {
        this.serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Delay(15000, stoppingToken); // workaround: necháme aplikaci nastartovat a spustit migrace
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = serviceProvider.CreateScope())
            {
                var deaktivaceOsobService = scope.ServiceProvider.GetRequiredService<IDeaktivaceOsobService>();
                deaktivaceOsobService.DeaktivujOsoby();
            }

            await Task.Delay(1000 * 60 * 60 * 6 /* 6 hodiny */, stoppingToken);
        }
    }
}
