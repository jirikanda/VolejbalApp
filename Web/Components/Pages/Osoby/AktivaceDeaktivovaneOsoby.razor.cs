using Havit.Blazor.Components.Web.Bootstrap;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Web.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Components.Pages.Osoby;

public partial class AktivaceDeaktivovaneOsoby
{
	[Inject]
	protected IOsobaWebApiClient OsobaWebApiClient { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	protected OsobaListDto osoby;

	private HxModal deleteModal;
	private OsobaDto osobaKeSmazani;

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		osoby = await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.GetNeaktivniOsobyAsync());
	}

	protected async Task AktivovatAsync(OsobaDto osoba)
	{
		await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.AktivujNeaktivniOsobuAsync(osoba.Id));
		osoby.Osoby.Remove(osoba);
	}

	protected async Task SmazatAsync(OsobaDto osoba)
	{
		osobaKeSmazani = osoba;
		await deleteModal.ShowAsync();
	}

	protected async Task PotvrditSmazaniAsync()
	{
		await deleteModal.HideAsync();
		await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.SmazNeaktivniOsobuAsync(osobaKeSmazani.Id));
		osoby.Osoby.Remove(osobaKeSmazani);
		osobaKeSmazani = null;
	}
}
