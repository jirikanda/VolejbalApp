using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.ProgressComponent;

public class Progress
{
	private readonly ProgressState progressState;
	private readonly Action stateHasChanged;
	private int counter = 0;

	public Progress(ProgressState progressState, Action stateHasChanged)
	{
		this.progressState = progressState;
		this.stateHasChanged = stateHasChanged;
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
		var stopwatch = Stopwatch.StartNew();
		try
		{

			Task<TResult> actionTask = action();
			if (await Task.WhenAny(actionTask, Task.Delay(100)) != actionTask)
			{
				lock (this)
				{
					counter += 1;
					progressState.InProgress = true;
					stateHasChanged();
				}
			}

			return await actionTask;
		}
		finally
		{
			stopwatch.Stop();

			if ((stopwatch.ElapsedMilliseconds < 300) && (counter == 1))
			{
				// TODO: Ale nechceme běh programu blokovat...
				await Task.Delay(300 - (int)stopwatch.ElapsedMilliseconds);
			}
			lock (this)
			{
				counter -= 1;
				progressState.InProgress = counter > 0;
				stateHasChanged();
			}
		}
	}
}
