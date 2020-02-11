using KandaEu.Volejbal.Web.Components.LoadingComponent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Nastenka
{
	public partial class Nastenka
	{
		private NastenkaFormData formData = new NastenkaFormData();
		private LoadingState LoadingState = new LoadingState();
		private NastenkaState State = new NastenkaState();

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
			try
			{
				LoadingState.LoadingInProgress = true;
				State.AktivniOsoby = await OsobaWebApiClient.GetAktivniOsobyAsync();
				State.Vzkazy = await NastenkaWebApiClient.GetVzkazyAsync();
			}
			catch
			{
				LoadingState.LoadingFailed = true;
			}
			finally
			{
				LoadingState.LoadingInProgress = false;
			}
		}
	}
}
