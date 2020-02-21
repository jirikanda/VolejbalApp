using KandaEu.Volejbal.Web.Components.ProgressComponent;
using KandaEu.Volejbal.Web.WebApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Osoby
{
	public partial class AktivaceDeaktivovaneOsoby
	{
		[Inject]
		protected IOsobaWebApiClient OsobaWebApiClient { get; set; }

		[CascadingParameter]
		protected Progress Progress { get; set; }

		[Inject]
		protected IJSRuntime JSRuntime { get; set; }

		protected OsobaListDto osoby;

		[Inject]
		protected Sotsera.Blazor.Toaster.IToaster Toaster { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			osoby = await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.GetNeaktivniOsobyAsync());
		}

		protected async Task Aktivovat(OsobaDto2 osoba)
		{
			await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.AktivujNeaktivniOsobuAsync(osoba.Id));
			osoby.Osoby.Remove(osoba);

			Toaster.Success($"{osoba.PrijmeniJmeno} aktivován(a).");
		}

		protected async Task Smazat(OsobaDto2 osoba)
		{
			bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"Opravdu chceš smazat osobu \"{osoba.PrijmeniJmeno}\"?");
			if (confirmed)
			{
				await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.SmazNeaktivniOsobuAsync(osoba.Id));
				osoby.Osoby.Remove(osoba);
				Toaster.Success($"{osoba.PrijmeniJmeno} smazán(a).");
			}
		}
	}
}
