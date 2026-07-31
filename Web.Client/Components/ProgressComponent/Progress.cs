using System.Diagnostics;

namespace KandaEu.Volejbal.Web.Client.Components.ProgressComponent;

public class Progress
{
	private readonly ProgressState _progressState;
	private readonly Action _stateHasChanged;
	private int _counter = 0;

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
		var stopwatch = Stopwatch.StartNew();
		try
		{

			Task<TResult> actionTask = action();
			if (await Task.WhenAny(actionTask, Task.Delay(100)) != actionTask)
			{
				lock (this)
				{
					_counter += 1;
					_progressState.InProgress = true;
					_stateHasChanged();
				}
			}

			return await actionTask;
		}
		finally
		{
			stopwatch.Stop();

			if ((stopwatch.ElapsedMilliseconds < 300) && (_counter == 1))
			{
				// TODO: Ale nechceme běh programu blokovat...
				await Task.Delay(300 - (int)stopwatch.ElapsedMilliseconds);
			}
			lock (this)
			{
				_counter -= 1;
				_progressState.InProgress = _counter > 0;
				_stateHasChanged();
			}
		}
	}
}
