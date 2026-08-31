using KandaEu.Volejbal.Contracts.Terminy.Dto;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Terminy;

public partial class Terminy
{
	[Inject] protected ITerminWebApiClient TerminWebApiClient { get; set; }

	[CascadingParameter] protected ProgressComponent.Progress Progress { get; set; }

	protected TerminyState State { get; set; } = new TerminyState();

	[Parameter] public EventCallback<int> CurrentTerminIdChanged { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		TerminListDto terminList = await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.GetTerminyAsync());
		State.Terminy = terminList.Terminy.ToList();

		if (State.Terminy.Count > 0)
		{
			await SetCurrentTerminIdAsync(State.Terminy[0].Id);
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
}
