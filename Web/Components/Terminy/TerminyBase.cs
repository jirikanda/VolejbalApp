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
	public class TerminyBase : ComponentBase
	{
		[Inject]
		public ITerminWebApiClient TerminWebApiClient { get; set; }
		
		[Inject]
		public IEventAggregator EventAggregator { get; set; }

		protected TerminyState State { get; } = new TerminyState();

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			State.IsLoading = true;

			KandaEu.Volejbal.Web.WebApiClients.TerminListDto terminList;
			try
			{
				terminList = await TerminWebApiClient.GetTerminyAsync();
			}
			catch
			{
				State.LoadingFailed = true;
				throw; // ???
			}
			finally
			{
				State.IsLoading = false;
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
