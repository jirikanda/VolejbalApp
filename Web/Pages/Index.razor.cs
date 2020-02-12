﻿using KandaEu.Volejbal.Web.WebApiClients;
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
		public INastenkaWebApiClient NastenkaWebApiClient { get; set; }
		
		[Inject]
		Blazored.LocalStorage.ILocalStorageService LocalStorageService { get; set; }

		protected bool ShowNastenkaLink { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (firstRender)
			{
				DateTime lastVisit = DateTime.Today.AddDays(-14);
				if (await LocalStorageService.ContainKeyAsync("LastVisit")) // JS Interop - musíme až do AfterRender
				{
					lastVisit = await LocalStorageService.GetItemAsync<DateTime>("LastVisit");
				}
				ShowNastenkaLink = (await NastenkaWebApiClient.GetVzkazyAsync()).Vzkazy.Any(vzkaz => vzkaz.DatumVlozeni > lastVisit);
				await LocalStorageService.SetItemAsync("LastVisit", DateTime.Now);
			}
		}
	}
}