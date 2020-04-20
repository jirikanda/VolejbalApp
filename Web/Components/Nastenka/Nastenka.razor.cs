using KandaEu.Volejbal.Contracts.Nastenka;
using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Web.Components.ProgressComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Nastenka
{
	public partial class Nastenka
	{
		private NovyVzkazFormData formData = new NovyVzkazFormData();
		private NastenkaState State = new NastenkaState();

		[CascadingParameter]
		public Progress Progress { get; set; }

		[Inject]
		protected Sotsera.Blazor.Toaster.IToaster Toaster { get; set; }

		[Inject]
		protected IJSRuntime JSRuntime { get; set; }

		[Inject]
		private INastenkaFacade NastenkaFacade { get; set; }

		[Inject]
		private IOsobaFacade OsobaFacade { get; set; }

		protected override async Task OnInitializedAsync()
		{
			await base.OnInitializedAsync();
			await LoadDataAsync();
		}

		private async Task OnValidSubmitAsync()
		{
			// TODO: Sdílení Contracts!		
			await NastenkaFacade.VlozVzkaz(new VzkazInputDto { AutorId = formData.AutorId.Value, Zprava = formData.Zprava });
			formData.Zprava = ""; // vyčistit formulář
			await LoadDataAsync();

			Toaster.Success("Vzkaz zapsán.");
		}

		private async Task LoadDataAsync()
		{
			State.AktivniOsoby = null;
			State.Vzkazy = null;

			await Progress.ExecuteInProgressAsync(async () =>
			{
				State.AktivniOsoby = (await OsobaFacade.GetAktivniOsoby()).Osoby.ToList();
				State.Vzkazy = (await NastenkaFacade.GetVzkazy()).Vzkazy.ToList();
			});
		}

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (firstRender)
			{
				await JSRuntime.InvokeVoidAsync("setTitle", "Volejbal - Nástěnka");
			}
		}
	}
}
