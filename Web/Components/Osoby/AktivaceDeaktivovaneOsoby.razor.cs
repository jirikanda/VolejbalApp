using KandaEu.Volejbal.Web.Components.ProgressComponent;
using KandaEu.Volejbal.Web.WebApiClients;
using Microsoft.AspNetCore.Components;
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

		protected OsobaListDto osoby;

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			osoby = await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.GetNeaktivniOsobyAsync());
		}

		protected async Task Aktivovat(OsobaDto2 osoba)
		{
			// TODO: TOASTER
			await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.AktivujNeaktivniOsobuAsync(osoba.Id));
			osoby.Osoby.Remove(osoba);
		}

		protected async Task Smazat(OsobaDto2 osoba)
		{
			// TODO: TOASTER
			await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.SmazNeaktivniOsobuAsync(osoba.Id));
			osoby.Osoby.Remove(osoba);
		}
	}
}
