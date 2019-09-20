using KandaEu.Volejbal.Web.Components.Terminy;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Prihlasovani
{
	public class PrihlasovaniBase : ComponentBase, EventAggregator.Blazor.IHandle<KandaEu.Volejbal.Web.Components.Terminy.CurrentTerminChanged>
	{
		[Inject]
		public IHttpClientFactory HttpClientFactory { get; set; }

		[Inject]
		public EventAggregator.Blazor.IEventAggregator EventAggregator { get; set; }

		protected PrihlasovaniState State { get; } = new PrihlasovaniState();

		protected override void OnInitialized()
		{
			base.OnInitialized();
			EventAggregator.Subscribe(this);
		}

		public async Task SetCurrentTermin(int terminId)
		{
			State.AktualniTerminId = terminId;

			State.IsLoading = true;
			State.Prihlaseni = null;
			State.Neprihlaseni = null;

			System.Threading.Thread.Sleep(1000);

			try
			{
				var terminDetail = await HttpClientFactory.CreateClient().GetJsonAsync<KandaEu.Volejbal.Contracts.Terminy.Dto.TerminDetailDto>($"http://localhost:9901/api/terminy/{terminId}");

				State.Prihlaseni = terminDetail.Prihlaseni;
				State.Neprihlaseni = terminDetail.Neprihlaseni;
			}
			catch
			{
				State.LoadingFailed = true;
			}
			finally
			{
				State.IsLoading = false;
			}

			StateHasChanged();
		}

		async Task EventAggregator.Blazor.IHandle<KandaEu.Volejbal.Web.Components.Terminy.CurrentTerminChanged>.HandleAsync(CurrentTerminChanged message)
		{
			await SetCurrentTermin(message.TerminId);
		}
	}
}
