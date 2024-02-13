using KandaEu.Volejbal.Contracts.Terminy.Dto;

// TODO: Není page!
namespace KandaEu.Volejbal.Web.Components.Pages.Terminy;

public partial class Terminy
{
	[Inject]
	protected ITerminWebApiClient TerminWebApiClient { get; set; }

	protected TerminyState State { get; } = new TerminyState();

	[CascadingParameter]
	protected ProgressComponent.Progress Progress { get; set; }

	[Parameter] public EventCallback<int> CurrentTerminIdChanged { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		TerminListDto terminList = await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.GetTerminyAsync());

		State.Terminy = terminList.Terminy.ToList();

		StateHasChanged();

		if (State.Terminy.Count > 0)
		{
			await SetCurrentTerminAsync(State.Terminy[0]);
		}
	}

	protected async Task TerminClickAsync(TerminDto termin)
	{
		await SetCurrentTerminAsync(termin);
	}

	private async Task SetCurrentTerminAsync(TerminDto termin)
	{
		State.CurrentTerminId = termin.Id;
		await CurrentTerminIdChanged.InvokeAsync(termin.Id);
	}
}
