using EventAggregator.Blazor;
using KandaEu.Volejbal.Web.WebApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Terminy
{
	public partial class Terminy
	{
		[Inject]
		protected ITerminWebApiClient TerminWebApiClient { get; set; }
		
		[Inject]
		protected IEventAggregator EventAggregator { get; set; }

		protected TerminyState State { get; } = new TerminyState();

		[CascadingParameter]
		protected ProgressComponent.Progress Progress { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			KandaEu.Volejbal.Web.WebApiClients.TerminListDto terminList = await Progress.ExecuteInProgressAsync(async () => await TerminWebApiClient.GetTerminyAsync());

			State.Terminy = terminList.Terminy.ToList();
			
			StateHasChanged();

			if (State.Terminy.Count > 0)
			{
				// TODO: Refactor (Extract)
				State.CurrentTerminId = State.Terminy[0].Id;
				await EventAggregator.PublishAsync(new CurrentTerminChanged(State.Terminy[0].Id));
			}
		}

		protected async Task TerminClickAsync(TerminDto termin)
		{
			// TODO: Refactor (Extract)
			State.CurrentTerminId = termin.Id;
			await EventAggregator.PublishAsync(new CurrentTerminChanged(termin.Id));
		}
	}
}
