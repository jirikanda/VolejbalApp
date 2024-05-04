using Hangfire;
using Hangfire.States;
using Havit.Hangfire.Extensions.BackgroundJobs;
using Havit.Hangfire.Extensions.RecurringJobs;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

		return Enumerable.Empty<IRecurringJob>();

	}
}