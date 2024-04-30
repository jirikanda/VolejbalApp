using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.Web.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Components.Pages.Prihlasovani;

// TODO: Není page!

public partial class Prihlasovani : ComponentBase, IDisposable
{
	[Inject]
	protected ITerminWebApiClient TerminWebApiClient { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	protected PrihlasovaniState State { get; } = new PrihlasovaniState();

	[Inject]
	protected Blazored.LocalStorage.ILocalStorageService LocalStorageService { get; set; }

	[Parameter] public int? CurrentTerminId { get; set; }
	protected int? PrefferedOsobaId { get; set; }

	private CancellationTokenSource _cancellationTokenSource;

	protected override async Task OnParametersSetAsync()
	{
		if ((CurrentTerminId != null) && (CurrentTerminId != State.AktualniTerminId))
		{
			await SetCurrentTerminAsync(CurrentTerminId.Value);
		}
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		await base.OnAfterRenderAsync(firstRender);
		if (firstRender)
		{
			if (await LocalStorageService.ContainKeyAsync("PrefferedOsobaId"))
			{
				this.PrefferedOsobaId = await LocalStorageService.GetItemAsync<int>("PrefferedOsobaId");
			}
		}
	}

	protected async Task SetCurrentTerminAsync(int terminId)
	{
		State.AktualniTerminId = terminId;

		State.Prihlaseni = null;
		State.Neprihlaseni = null;
		StateHasChanged();

		if (_cancellationTokenSource != null)
		{
			await _cancellationTokenSource.CancelAsync();
		}
		_cancellationTokenSource = new CancellationTokenSource();

		CancellationToken cancellationToken = _cancellationTokenSource.Token;
		try
		{
			TerminDetailDto terminDetail = await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.GetDetailTerminuAsync(terminId, cancellationToken));

			State.Prihlaseni = terminDetail.Prihlaseni.ToList();
			State.Neprihlaseni = terminDetail.Neprihlaseni.ToList();
		}
		catch (OperationCanceledException oce) when (oce.CancellationToken == cancellationToken)
		{
			// NOOP - zamaskujeme výjimku
		}
	}

	private async Task HandlePrihlasitClickAsync(NeprihlasenaOsobaDto prihlasovanaOsoba)
	{
		// pokud kliknu na přihlášení a následně na změnu termínu, dojde
		// - ke spuštění volání API pro přihlášení
		// - k nastavení State.Prihlaseni na null
		// - a k spuštění volní API pro načtení detailů termínu.
		// Pokud dojde k dokončení přihlášení před načtením termínu, je již State.Prihlaseni a State.Neprihlaseni null.
		// Pokud dokde k dokončení přihlášení po načtení termíu, jsou ve State.Prihlaseni a State.Neprihlaseni hodnoty nového termínu.
		// Takže nemůžeme volat Add/Remove nad State.Prihlaseni. Potřebujeme je volat nad kolekcemi platnými před spuštěním přihlašování.

		var prihlaseni = State.Prihlaseni;
		var neprihlaseni = State.Neprihlaseni;

		await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.PrihlasitAsync(State.AktualniTerminId.Value, prihlasovanaOsoba.Osoba.Id));

		neprihlaseni.RemoveAll(neprihlaseny => neprihlaseny.Osoba.Id == prihlasovanaOsoba.Osoba.Id);
		prihlaseni.RemoveAll(prihlaseny => prihlaseny.Osoba.Id == prihlasovanaOsoba.Osoba.Id); // to se snad nemůže stát
		prihlaseni.Add(new PrihlasenaOsobaDto { Osoba = prihlasovanaOsoba.Osoba });
		prihlaseni.Sort((a, b) => a.Osoba.PrijmeniJmeno.CompareTo(b.Osoba.PrijmeniJmeno));

		//Toaster.Success($"{neprihlaseny.PrijmeniJmeno} přihlášen(a).");

		await LocalStorageService.SetItemAsync("PrefferedOsobaId", prihlasovanaOsoba.Osoba.Id);
		PrefferedOsobaId = prihlasovanaOsoba.Osoba.Id;
	}

	private async Task HandleOdhlasitClickAsync(OsobaDto odhlasovanaOsobaDto)
	{
		var prihlaseni = State.Prihlaseni;
		var neprihlaseni = State.Neprihlaseni;

		await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.OdhlasitAsync(State.AktualniTerminId.Value, odhlasovanaOsobaDto.Id));

		prihlaseni.RemoveAll(prihlaseny => prihlaseny.Osoba.Id == odhlasovanaOsobaDto.Id);
		neprihlaseni.RemoveAll(item => item.Osoba.Id == odhlasovanaOsobaDto.Id);

		neprihlaseni.Add(new NeprihlasenaOsobaDto { Osoba = odhlasovanaOsobaDto, IsOdhlaseny = true });
		neprihlaseni.Sort((a, b) => a.Osoba.PrijmeniJmeno.CompareTo(b.Osoba.PrijmeniJmeno));

		//Toaster.Success($"{prihlaseny.PrijmeniJmeno} odhlášen(a).");
	}

	public void Dispose()
	{
		_cancellationTokenSource?.Dispose();
		_cancellationTokenSource = null;
	}
}
