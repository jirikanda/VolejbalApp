namespace KandaEu.Volejbal.Web.Client.Components.Pages;

public partial class Home
{
	[Inject]
	protected INastenkaWebApiClient NastenkaWebApiClient { get; set; }

	[Inject]
	protected Blazored.LocalStorage.ILocalStorageService LocalStorageService { get; set; }

	private int? _currentTerminId;

	protected bool ShowNastenkaLink { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		DateTime lastVisit = DateTime.Today.AddDays(-14);
		if (await LocalStorageService.ContainKeyAsync("LastVisit"))
		{
			lastVisit = await LocalStorageService.GetItemAsync<DateTime>("LastVisit");
		}

		if ((await NastenkaWebApiClient.GetVzkazyAsync()).Vzkazy.Any(vzkaz => vzkaz.DatumVlozeni > lastVisit))
		{
			ShowNastenkaLink = true;
		}

		await LocalStorageService.SetItemAsync("LastVisit", DateTime.Now);
	}

	private void HandleCurrentTerminIdChanged(int newCurrentterminId)
	{
		_currentTerminId = newCurrentterminId;
	}
}