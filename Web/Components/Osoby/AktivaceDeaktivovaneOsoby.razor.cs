using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Web.Components.ProgressComponent;
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
		protected IOsobaFacade OsobaFacade { get; set; }

		[CascadingParameter]
		protected Progress Progress { get; set; }

		[Inject]
		protected IJSRuntime JSRuntime { get; set; }

		protected List<OsobaDto> osoby;

		[Inject]
		protected Sotsera.Blazor.Toaster.IToaster Toaster { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			osoby = (await Progress.ExecuteInProgressAsync(async () => await OsobaFacade.GetNeaktivniOsoby())).Osoby;
		}

		protected async Task Aktivovat(OsobaDto osoba)
		{
			await Progress.ExecuteInProgressAsync(async () => await OsobaFacade.AktivujNeaktivniOsobu(new AktivujNeaktivniOsobuRequest { OsobaId = osoba.Id }));
			osoby.Remove(osoba);

			Toaster.Success($"{osoba.PrijmeniJmeno} aktivován(a).");
		}

		protected async Task Smazat(OsobaDto osoba)
		{
			bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"Opravdu chceš smazat osobu \"{osoba.PrijmeniJmeno}\"?");
			if (confirmed)
			{
				await Progress.ExecuteInProgressAsync(async () => await OsobaFacade.SmazNeaktivniOsobu(new SmazNeaktivniOsobuRequest { OsobaId = osoba.Id }));
				osoby.Remove(osoba);
				Toaster.Success($"{osoba.PrijmeniJmeno} smazán(a).");
			}
		}
	}
}
