using System.Diagnostics;

namespace KandaEu.Volejbal.Web.Client.Components.ProgressComponent;

/// <summary>
/// Řídí zobrazení indikátoru načítání kolem asynchronních operací.
///
/// Pravidla jsou dvě a obě existují proti problikávání:
/// - Spinner se zapne až když operace běží déle než <see cref="SpinnerDelayMilliseconds"/>. Rychlá volání
///   (typicky zahřátý server) tak neukážou vůbec nic.
/// - Jakmile se spinner jednou ukáže, zůstane aspoň <see cref="MinimumVisibleMilliseconds"/>.
///
/// Minimální doba zobrazení se uplatní jen tehdy, když spinner opravdu naběhl — rychlou cestu tedy
/// nic uměle nezdržuje. Čítač souběžných operací se ze stejného důvodu zvyšuje i snižuje jen kolem
/// skutečně zobrazeného spinneru, aby zůstal symetrický.
/// </summary>
public class Progress
{
	private const int SpinnerDelayMilliseconds = 100;
	private const int MinimumVisibleMilliseconds = 300;

	private readonly ProgressState _progressState;
	private readonly Action _stateHasChanged;

	/// <summary>
	/// Počet operací, které aktuálně zobrazují spinner. Bez zámku — Blazor WebAssembly renderuje
	/// v jednovláknovém synchronizačním kontextu, takže se sem nikdy nedostanou dvě vlákna naráz.
	/// </summary>
	private int _visibleCounter = 0;

	public Progress(ProgressState progressState, Action stateHasChanged)
	{
		_progressState = progressState;
		_stateHasChanged = stateHasChanged;
	}

	public async Task ExecuteInProgressAsync(Func<Task> action)
	{
		await ExecuteInProgressAsync<object>(async () =>
		{
			await action();
			return null;
		});
	}

	public async Task<TResult> ExecuteInProgressAsync<TResult>(Func<Task<TResult>> action)
	{
		Task<TResult> actionTask = action();

		if (!await WaitForSpinnerDelayAsync(actionTask))
		{
			// Operace se stihla pod SpinnerDelayMilliseconds — spinner se nezapnul, takže není co skrývat
			// ani jak dlouho držet.
			return await actionTask;
		}

		ShowSpinner();
		Stopwatch stopwatch = Stopwatch.StartNew();
		try
		{
			return await actionTask;
		}
		finally
		{
			stopwatch.Stop();
			int remainingMilliseconds = MinimumVisibleMilliseconds - (int)stopwatch.ElapsedMilliseconds;
			if (remainingMilliseconds > 0)
			{
				await Task.Delay(remainingMilliseconds);
			}
			HideSpinner();
		}
	}

	/// <summary>
	/// Vrací true, pokud operace po uplynutí <see cref="SpinnerDelayMilliseconds"/> stále běží
	/// (a spinner je tedy potřeba zobrazit).
	/// </summary>
	private static async Task<bool> WaitForSpinnerDelayAsync(Task actionTask)
	{
		// CancellationTokenSource proto, aby po dokončení operace nezůstal viset timer čekající
		// na doběhnutí Task.Delay.
		using CancellationTokenSource delayCancellationTokenSource = new CancellationTokenSource();
		Task delayTask = Task.Delay(SpinnerDelayMilliseconds, delayCancellationTokenSource.Token);
		try
		{
			return await Task.WhenAny(actionTask, delayTask) != actionTask;
		}
		finally
		{
			await delayCancellationTokenSource.CancelAsync();
		}
	}

	private void ShowSpinner()
	{
		_visibleCounter += 1;
		_progressState.InProgress = true;
		_stateHasChanged();
	}

	private void HideSpinner()
	{
		_visibleCounter -= 1;
		_progressState.InProgress = _visibleCounter > 0;
		_stateHasChanged();
	}
}
