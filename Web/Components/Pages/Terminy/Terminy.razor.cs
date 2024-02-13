using System;
using KandaEu.Volejbal.Contracts.Terminy.Dto;

// TODO: Není page!
namespace KandaEu.Volejbal.Web.Components.Pages.Terminy;

public partial class Terminy : IDisposable
{
	[Inject] protected ITerminWebApiClient TerminWebApiClient { get; set; }

	[Inject] protected  PersistentComponentState ApplicationState { get; set; }

	[CascadingParameter] protected ProgressComponent.Progress Progress { get; set; }

	protected TerminyState State { get; set; } = new TerminyState();

	[Parameter] public EventCallback<int> CurrentTerminIdChanged { get; set; }

	private PersistingComponentStateSubscription _persistingSubscription;

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		_persistingSubscription = ApplicationState.RegisterOnPersisting(PersistDataAsync);

		if (ApplicationState.TryTakeFromJson<TerminyState>("State", out TerminyState restoredState))
		{
			State = restoredState;
			if (State.CurrentTerminId != null)
			{
				await SetCurrentTerminIdAsync(State.CurrentTerminId.Value);
			}
		}
		else
		{
			TerminListDto terminList = await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.GetTerminyAsync());
			State.Terminy = terminList.Terminy.ToList();

			if (State.Terminy.Count > 0)
			{
				await SetCurrentTerminIdAsync(State.Terminy[0].Id);
			}
		}
	}

	protected async Task TerminClickAsync(TerminDto termin)
	{
		await SetCurrentTerminIdAsync(termin.Id);
	}

	private async Task SetCurrentTerminIdAsync(int terminId)
	{
		State.CurrentTerminId = terminId;
		await CurrentTerminIdChanged.InvokeAsync(terminId);
	}

	private Task PersistDataAsync()
	{
		ApplicationState.PersistAsJson("State", State);
		return Task.CompletedTask;
	}

	public void Dispose()
	{
		_persistingSubscription.Dispose();
	}
}
