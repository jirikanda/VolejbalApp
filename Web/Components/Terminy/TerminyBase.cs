using EventAggregator.Blazor;
using KandaEu.Volejbal.Web.Components.LoadingComponent;
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
	public class TerminyBase : ComponentBase
	{
		[Inject]
		public ITerminWebApiClient TerminWebApiClient { get; set; }
		
		[Inject]
		public IEventAggregator EventAggregator { get; set; }

		protected LoadingState LoadingState { get; } = new LoadingState();
		protected TerminyState State { get; } = new TerminyState();

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			LoadingState.LoadingInProgress = true;

			KandaEu.Volejbal.Web.WebApiClients.TerminListDto terminList;
			try
			{
				terminList = await TerminWebApiClient.GetTerminyAsync();
			}
			catch
			{
				LoadingState.LoadingFailed = true;
				throw; // ???
			}
			finally
			{
				LoadingState.LoadingInProgress = false;
			}

			State.Terminy = terminList.Terminy.ToList();
			
			StateHasChanged();

			if (State.Terminy.Count > 0)
			{
				// TODO: Refactor (Extract)
				State.CurrentTerminId = State.Terminy[0].Id;
				await EventAggregator.PublishAsync(new CurrentTerminChanged(State.Terminy[0].Id));
			}
		}

		protected Func<MouseEventArgs, Task> XButtonClick(int terminId)
		{
			return async (MouseEventArgs e) =>
			{
				// TODO: Refactor (Extract)
				State.CurrentTerminId = terminId;
				await EventAggregator.PublishAsync(new CurrentTerminChanged(terminId));
			};
		}
	}
}
