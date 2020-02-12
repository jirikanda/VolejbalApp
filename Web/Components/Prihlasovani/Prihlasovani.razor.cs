using KandaEu.Volejbal.Web.Components.ProgressComponent;
using KandaEu.Volejbal.Web.Components.Terminy;
using KandaEu.Volejbal.Web.WebApiClients;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
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

		protected override void OnInitialized()
		{
			base.OnInitialized();
			EventAggregator.Subscribe(this);
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
			await TerminWebApiClient.PrihlasitAsync(State.AktualniTerminId.Value, neprihlaseny.Id);
			State.Prihlaseni.Add(neprihlaseny);
			State.Neprihlaseni.Remove(neprihlaseny);
		}

		private async Task Odhlasit(OsobaDto prihlaseny)
		{
			await TerminWebApiClient.OdhlasitAsync(State.AktualniTerminId.Value, prihlaseny.Id);
			State.Neprihlaseni.Add(prihlaseny);
			State.Prihlaseni.Remove(prihlaseny);
		}
	}
}
