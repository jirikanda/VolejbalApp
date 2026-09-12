namespace KandaEu.Volejbal.Web.Client.Components.Pages;

public partial class Home
{
	[Inject]
	protected INastenkaApi NastenkaApi { get; set; }

	[Inject]
	protected Havit.Blazor.Storage.ILocalStorageService LocalStorageService { get; set; }

	private int? _currentTerminId;

	protected bool ShowNastenkaLink { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();

		(bool Success, DateTime Value) lastVisitResult = await LocalStorageService.TryGetValueAsync<DateTime>("LastVisit");
		DateTime lastVisit = lastVisitResult.Success ? lastVisitResult.Value : DateTime.Today.AddDays(-14);

		if ((await NastenkaApi.GetVzkazyAsync()).Vzkazy.Any(vzkaz => vzkaz.DatumVlozeni > lastVisit))
		{
			ShowNastenkaLink = true;
		}

		await LocalStorageService.SetValueAsync("LastVisit", DateTime.Now);
	}

	private void HandleCurrentTerminIdChanged(int newCurrentterminId)
	{
		_currentTerminId = newCurrentterminId;
	}
}