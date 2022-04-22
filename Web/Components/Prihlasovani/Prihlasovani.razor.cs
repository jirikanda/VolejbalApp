using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.Web.Components.ProgressComponent;
using KandaEu.Volejbal.Web.Components.Terminy;
using KandaEu.Volejbal.Web.WebApiClients;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Prihlasovani
{
	public partial class Prihlasovani : ComponentBase, EventAggregator.Blazor.IHandle<KandaEu.Volejbal.Web.Components.Terminy.CurrentTerminChanged>
	{
		[Inject]
		protected ITerminWebApiClient TerminWebApiClient { get; set; }

		[Inject]
		protected EventAggregator.Blazor.IEventAggregator EventAggregator { get; set; }

		[CascadingParameter]
		protected Progress Progress { get; set; }

		protected PrihlasovaniState State { get; } = new PrihlasovaniState();

		[Inject]
		protected Blazored.LocalStorage.ILocalStorageService LocalStorageService { get; set; }

		[Inject]
		protected Sotsera.Blazor.Toaster.IToaster Toaster { get; set; }

		protected int? PrefferedOsobaId { get; set; }

		protected override void OnInitialized()
		{
			base.OnInitialized();
			EventAggregator.Subscribe(this);
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

		protected async Task SetCurrentTermin(int terminId)
		{
			State.AktualniTerminId = terminId;

			State.Prihlaseni = null;
			State.Neprihlaseni = null;
			StateHasChanged();

			TerminDetailDto terminDetail = await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.GetDetailTerminuAsync(terminId));

			State.Prihlaseni = terminDetail.Prihlaseni.ToList();
			State.Neprihlaseni = terminDetail.Neprihlaseni.ToList();

			StateHasChanged();
		}

		async Task EventAggregator.Blazor.IHandle<KandaEu.Volejbal.Web.Components.Terminy.CurrentTerminChanged>.HandleAsync(CurrentTerminChanged message)
		{
			await SetCurrentTermin(message.TerminId);
		}

		private async Task Prihlasit(OsobaDto neprihlaseny)
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

			await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.PrihlasitAsync(State.AktualniTerminId.Value, neprihlaseny.Id));

			if (!prihlaseni.Contains(neprihlaseny))
			{
				prihlaseni.Add(neprihlaseny);
			}
			neprihlaseni.Remove(neprihlaseny);

			Toaster.Success($"{neprihlaseny.PrijmeniJmeno} přihlášen(a).");
			
			await LocalStorageService.SetItemAsync("PrefferedOsobaId", neprihlaseny.Id);
			PrefferedOsobaId = neprihlaseny.Id;
		}

		private async Task Odhlasit(OsobaDto prihlaseny)
		{
			var prihlaseni = State.Prihlaseni;
			var neprihlaseni = State.Neprihlaseni;

			await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.OdhlasitAsync(State.AktualniTerminId.Value, prihlaseny.Id));

			if (!neprihlaseni.Contains(prihlaseny)) // pokud došlo k doubleclicku, mohl se tam dostat
			{
				neprihlaseni.Add(prihlaseny);
				neprihlaseni.Sort((a, b) => a.PrijmeniJmeno.CompareTo(b.PrijmeniJmeno));
			}
			prihlaseni.Remove(prihlaseny);

			Toaster.Success($"{prihlaseny.PrijmeniJmeno} odhlášen(a).");
		}
	}
}
