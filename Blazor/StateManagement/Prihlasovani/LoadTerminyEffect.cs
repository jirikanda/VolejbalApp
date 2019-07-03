using Blazor.Fluxor;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{
	public class LoadTerminyEffect : Effect<LoadTerminyAction>
	{
		private readonly HttpClient httpClient;
		private readonly IState<TerminyState> terminState;

		public LoadTerminyEffect(HttpClient httpClient, IState<TerminyState> terminState)
		{
			this.httpClient = httpClient;
			this.terminState = terminState;
		}

		protected override async Task HandleAsync(LoadTerminyAction action, IDispatcher dispatcher)
		{
			if (terminState.Value.Terminy != null)
			{
				return;
			}

			try
			{
				var terminList = await httpClient.GetJsonAsync<TerminListDto>("http://localhost:9901/api/terminy");
				System.Threading.Thread.Sleep(1000);
				dispatcher.Dispatch(new LoadTerminySuccessAction(terminList.Terminy));
			}
			catch (Exception e)
			{
				dispatcher.Dispatch(new LoadTerminyFailedAction());
			}
		}
	}
}
