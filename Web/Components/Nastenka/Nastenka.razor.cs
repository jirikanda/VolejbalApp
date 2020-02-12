using KandaEu.Volejbal.Web.Components.ProgressComponent;
using Microsoft.AspNetCore.Components;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Nastenka
{
	public partial class Nastenka
	{
		private NastenkaFormData formData = new NastenkaFormData();
		private NastenkaState State = new NastenkaState();

		[CascadingParameter]
		public Progress Progress { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			await LoadDataAsync();
		}

		private async Task OnValidSubmitAsync()
		{
			// TODO: Sdílení Contracts!		
			await NastenkaWebApiClient.VlozVzkazAsync(new WebApiClients.VzkazInputDto { AutorId = formData.AutorId.Value, Zprava = formData.Zprava });
			formData.Zprava = ""; // vyčistit formulář
			await LoadDataAsync();
		}

		private async Task LoadDataAsync()
		{
			State.AktivniOsoby = null;
			State.Vzkazy = null;


			await Progress.ExecuteInProgressAsync(async () =>
			{
				State.AktivniOsoby = await OsobaWebApiClient.GetAktivniOsobyAsync();
				State.Vzkazy = await NastenkaWebApiClient.GetVzkazyAsync();
			});
		}
	}
}
