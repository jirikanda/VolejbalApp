using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Web.Client.Components.ProgressComponent;
using Microsoft.AspNetCore.Components.Web;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Osoby;

public partial class SeznamOsob
{
	[Inject]
	protected IOsobaApi OsobaApi { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	private OsobaListDto _osoby;

	private OsobaDto _osobaKeSmazani;
	private ElementReference _modalBackdrop;
	private bool _modalCekaNaFocus;

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		_osoby = await Progress.ExecuteInProgressAsync(async () => await OsobaApi.GetOsobyAsync());
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);

		// Backdrop musí dostat focus, jinak se k němu nedostane @onkeydown a nefunguje zavření Escapem.
		if (_modalCekaNaFocus)
		{
			_modalCekaNaFocus = false;
			await _modalBackdrop.FocusAsync();
		}
	}

	protected async Task AktivovatAsync(OsobaDto osoba)
	{
		await Progress.ExecuteInProgressAsync(async () => await OsobaApi.AktivujOsobuAsync(osoba.Id));
		osoba.Aktivni = true;
	}

	protected async Task DeaktivovatAsync(OsobaDto osoba)
	{
		await Progress.ExecuteInProgressAsync(async () => await OsobaApi.DeaktivujOsobuAsync(osoba.Id));
		osoba.Aktivni = false;
	}

	protected void ZobrazPotvrzeniSmazani(OsobaDto osoba)
	{
		_osobaKeSmazani = osoba;
		_modalCekaNaFocus = true;
	}

	protected void ZavriPotvrzeniSmazani()
	{
		_osobaKeSmazani = null;
	}

	protected void HandleModalKeyDown(KeyboardEventArgs keyboardEventArgs)
	{
		if (keyboardEventArgs.Key == "Escape")
		{
			ZavriPotvrzeniSmazani();
		}
	}

	protected async Task PotvrditSmazaniAsync()
	{
		OsobaDto osobaKeSmazani = _osobaKeSmazani;
		_osobaKeSmazani = null;

		await Progress.ExecuteInProgressAsync(async () => await OsobaApi.SmazOsobuAsync(osobaKeSmazani.Id));
		_osoby.Osoby.Remove(osobaKeSmazani);
	}
}
