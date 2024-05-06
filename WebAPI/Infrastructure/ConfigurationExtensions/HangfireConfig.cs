using Hangfire;
using Hangfire.States;
using Havit.Hangfire.Extensions.BackgroundJobs;
using Havit.Hangfire.Extensions.RecurringJobs;
using KandaEu.Volejbal.Services.Jobs;
using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

namespace KandaEu.Volejbal.WebAPI.Infrastructure.ConfigurationExtensions;

public static class HangfireConfig
{
	public static IServiceCollection AddCustomizedHangfireServer(this IServiceCollection services)
	{
#if DEBUG
		services.AddHangfireEnqueuedJobsCleanupOnApplicationStartup();
#endif
		services.AddHangfireRecurringJobsSchedulerOnApplicationStartup(GetRecurringJobsToSchedule().ToArray());

		// Add the processing server as IHostedService
		services.AddHangfireServer(o =>
		{
			o.WorkerCount = 1;
			o.Queues = new string[] { EnqueuedState.DefaultQueue }; // explicitně vyjadřujeme obsluhu výchozí fronty
		});

		return services;
	}

	private static IEnumerable<IRecurringJob> GetRecurringJobsToSchedule()
	{
		TimeZoneInfo timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");

		yield return new RecurringJob<IEnsureTerminyJob>(x => x.ExecuteAsync(CancellationToken.None), Cron.Hourly(), timeZone);
		yield return new RecurringJob<IDeaktivaceOsobJob>(x => x.ExecuteAsync(CancellationToken.None), Cron.Daily(4, 00), timeZone);
		yield return new RecurringJob<IPripomenutiPrihlaskyJob>(x => x.ExecuteAsync(CancellationToken.None), Cron.Daily(16, 00), timeZone);
	}
}