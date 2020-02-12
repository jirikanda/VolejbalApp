using KandaEu.Volejbal.Web.WebApiClients;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Pages
{
	public partial class Index
	{
		[Inject]
		INastenkaWebApiClient NastenkaWebApiClient { get; set; }

		protected bool ShowNastenkaLink { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			ShowNastenkaLink = (await NastenkaWebApiClient.GetVzkazyAsync()).Vzkazy.Any();
		}

	}
}
