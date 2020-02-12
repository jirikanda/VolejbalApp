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
		public IOsobaWebApiClient OsobaWebApiClient { get; set; }

		[CascadingParameter]
		private Progress Progress { get; set; }

		OsobaListDto osoby;

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			osoby = await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.GetNeaktivniOsobyAsync());
		}

		private async Task Aktivovat(OsobaDto2 osoba)
		{
			// TODO: TOASTER
			await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.AktivujNeaktivniOsobuAsync(osoba.Id));
			osoby.Osoby.Remove(osoba);
		}

		private async Task Smazat(OsobaDto2 osoba)
		{
			// TODO: TOASTER
			await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.SmazNeaktivniOsobuAsync(osoba.Id));
			osoby.Osoby.Remove(osoba);
		}
	}
}
