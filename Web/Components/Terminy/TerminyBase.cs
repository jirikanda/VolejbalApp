using EventAggregator.Blazor;
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
	public class TerminyBase : ComponentBase
	{
		[Inject]
		public IHttpClientFactory HttpClientFactory { get; set; }
		
		[Inject]
		public IEventAggregator EventAggregator { get; set; }

		protected TerminyState State { get; } = new TerminyState();

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();

			State.IsLoading = true;

			TerminListDto terminList;
			try
			{
				terminList = await HttpClientFactory.CreateClient().GetJsonAsync<TerminListDto>("http://localhost:9901/api/terminy");
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
			State.Terminy = terminList.Terminy;
			
			StateHasChanged();

			if (State.Terminy.Count > 0)
			{
				await EventAggregator.PublishAsync(new CurrentTerminChanged(State.Terminy[0].Id));
			}
		}

		protected Func<MouseEventArgs, Task> XButtonClick(int terminId)
		{
			return async (MouseEventArgs e) =>
			{
				await EventAggregator.PublishAsync(new CurrentTerminChanged(terminId));
			};
		}
	}
}
