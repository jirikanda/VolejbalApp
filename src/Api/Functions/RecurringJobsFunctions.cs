using KandaEu.Volejbal.Services.Terminy.EnsureTerminy;
using Microsoft.Azure.Functions.Worker;

namespace KandaEu.Volejbal.Api.Functions;

/// <summary>
/// Pravidelné úlohy.
/// </summary>
/// <remarks>
/// Nahrazuje RecurringJobsBackgroundService z ASP.NET Core hostingu. Plánovač už není in-process,
/// takže nic nevyžaduje běh v jediné instanci.
///
/// IDeaktivaceOsobJob se záměrně neplánuje - automatická deaktivace po dvou měsících neúčasti byla
/// zrušena, deaktivace je ruční akcí ve správě hráčů. Job zůstává pro ruční použití.
/// </remarks>
public class RecurringJobsFunctions(IEnsureTerminyJob _ensureTerminyJob)
{
	/// <summary>
	/// Doplnění termínů, každou hodinu. NCRONTAB má šest polí - první jsou sekundy.
	/// </summary>
	[Function(nameof(EnsureTerminyAsync))]
	public async Task EnsureTerminyAsync(
		[TimerTrigger("0 0 * * * *")] TimerInfo timer,
		CancellationToken cancellationToken)
	{
		await _ensureTerminyJob.ExecuteAsync(cancellationToken);
	}
}
