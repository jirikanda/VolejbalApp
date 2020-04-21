using EventAggregator.Blazor;
using KandaEu.Volejbal.Contracts.Terminy;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
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
		protected ITerminFacade TerminFacade { get; set; }
		
		[Inject]
		protected IEventAggregator EventAggregator { get; set; }

		protected TerminyState State { get; } = new TerminyState();

		[CascadingParameter]
		protected ProgressComponent.Progress Progress { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			GetTerminyResponse terminList = await Progress.ExecuteInProgressAsync(async () => await TerminFacade.GetTerminy());

			State.Terminy = terminList.Terminy;
			
			StateHasChanged();

			if (State.Terminy.Count > 0)
			{
				await SetCurrentTerminAsync(State.Terminy[0]);
			}
		}

		protected async Task TerminClickAsync(TerminDto termin)
		{
			await SetCurrentTerminAsync(termin);
		}

		private async Task SetCurrentTerminAsync(TerminDto termin)
		{
			State.CurrentTerminId = termin.Id;
			await EventAggregator.PublishAsync(new CurrentTerminChanged(termin.Id));
		}
	}
}
