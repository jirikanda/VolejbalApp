using KandaEu.Volejbal.Web.Components.ProgressComponent;
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
		public IOsobaWebApiClient OsobaWebApiClient { get; set; }

		[Inject]
		public NavigationManager NavigationManager { get; set; }

		[CascadingParameter]
		public Progress Progress { get; set; }

		public NovaOsobaFormData formData = new NovaOsobaFormData();
		
		private async Task ValidSubmit()
		{
			OsobaInputDto novaOsoba = new OsobaInputDto()
			{
				Jmeno = formData.Jmeno,
				Prijmeni = formData.Prijmeni,
				Email = formData.Email
			};

			await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.VlozOsobuAsync(novaOsoba));
			NavigationManager.NavigateTo("/");
		}
	}
}
