﻿using KandaEu.Volejbal.Web.Components.ProgressComponent;
using KandaEu.Volejbal.Web.WebApiClients;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Osoby
{
	public partial class ZalozeniNoveOsoby
	{
		[Inject]
		protected IOsobaWebApiClient OsobaWebApiClient { get; set; }

		[Inject]
		protected NavigationManager NavigationManager { get; set; }

		[Inject]
		protected Sotsera.Blazor.Toaster.IToaster Toaster { get; set; }

		[CascadingParameter]
		protected Progress Progress { get; set; }

		protected NovaOsobaFormData formData = new NovaOsobaFormData();

		protected async Task ValidSubmit()
		{
			OsobaInputDto novaOsoba = new OsobaInputDto()
			{
				Jmeno = formData.Jmeno,
				Prijmeni = formData.Prijmeni,
				Email = formData.Email
			};

			await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.VlozOsobuAsync(novaOsoba));
			Toaster.Success($"{novaOsoba.Prijmeni} {novaOsoba.Jmeno} založen(a).");
			NavigationManager.NavigateTo("/");
		}
	}
}
