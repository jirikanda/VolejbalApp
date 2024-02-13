using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Web.Components.ProgressComponent;
using Microsoft.JSInterop;

namespace KandaEu.Volejbal.Web.Components.Pages.Osoby;

public partial class AktivaceDeaktivovaneOsoby
{
	[Inject]
	protected IOsobaWebApiClient OsobaWebApiClient { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	[Inject]
	protected IJSRuntime JSRuntime { get; set; }

	protected OsobaListDto osoby;

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		osoby = await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.GetNeaktivniOsobyAsync());
	}

	protected async Task AktivovatAsync(OsobaDto osoba)
	{
		await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.AktivujNeaktivniOsobuAsync(osoba.Id));
		osoby.Osoby.Remove(osoba);

		//Toaster.Success($"{osoba.PrijmeniJmeno} aktivován(a).");
	}

	protected async Task SmazatAsync(OsobaDto osoba)
	{
		bool confirmed = await JSRuntime.InvokeAsync<bool>("confirm", $"Opravdu chceš smazat osobu \"{osoba.PrijmeniJmeno}\"?");
		if (confirmed)
		{
			await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.SmazNeaktivniOsobuAsync(osoba.Id));
			osoby.Osoby.Remove(osoba);
			//Toaster.Success($"{osoba.PrijmeniJmeno} smazán(a).");
		}
	}
}
