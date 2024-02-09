using KandaEu.Volejbal.Web.WebApiClients;
using Microsoft.AspNetCore.Components;

namespace KandaEu.Volejbal.Web.Components;

public partial class Index
{
	[Inject]
	protected INastenkaWebApiClient NastenkaWebApiClient { get; set; }

	[Inject]
	protected Blazored.LocalStorage.ILocalStorageService LocalStorageService { get; set; }

	public int? currentTerminId;

	protected bool ShowNastenkaLink { get; set; }

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			DateTime lastVisit = DateTime.Today.AddDays(-14);
			if (await LocalStorageService.ContainKeyAsync("LastVisit")) // JS Interop - musíme až do AfterRender
			{
				lastVisit = await LocalStorageService.GetItemAsync<DateTime>("LastVisit");
			}

			if ((await NastenkaWebApiClient.GetVzkazyAsync()).Vzkazy.Any(vzkaz => vzkaz.DatumVlozeni > lastVisit))
			{
				ShowNastenkaLink = true;
				StateHasChanged();
			}

			await LocalStorageService.SetItemAsync("LastVisit", DateTime.Now);
		}
	}

	private void HandleCurrentTerminIdChanged(int newCurrentterminId)
	{
		currentTerminId = newCurrentterminId;
	}
}
