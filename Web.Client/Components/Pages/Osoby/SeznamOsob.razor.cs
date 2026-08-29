using Havit.Blazor.Components.Web.Bootstrap;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Web.Client.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Osoby;

public partial class SeznamOsob
{
	[Inject]
	protected IOsobaWebApiClient OsobaWebApiClient { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	private OsobaListDto _osoby;

	private HxModal _deleteModal;
	private OsobaDto _osobaKeSmazani;

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		_osoby = await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.GetOsobyAsync());
	}

	protected async Task AktivovatAsync(OsobaDto osoba)
	{
		await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.AktivujOsobuAsync(osoba.Id));
		osoba.Aktivni = true;
	}

	protected async Task DeaktivovatAsync(OsobaDto osoba)
	{
		await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.DeaktivujOsobuAsync(osoba.Id));
		osoba.Aktivni = false;
	}

	protected async Task SmazatAsync(OsobaDto osoba)
	{
		_osobaKeSmazani = osoba;
		await _deleteModal.ShowAsync();
	}

	protected async Task PotvrditSmazaniAsync()
	{
		await _deleteModal.HideAsync();
		await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.SmazOsobuAsync(_osobaKeSmazani.Id));
		_osoby.Osoby.Remove(_osobaKeSmazani);
		_osobaKeSmazani = null;
	}
}
