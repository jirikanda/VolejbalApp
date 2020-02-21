using KandaEu.Volejbal.Web.Components.ProgressComponent;
using KandaEu.Volejbal.Web.WebApiClients;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Osoby
{
	public partial class Osoby
	{
		[Inject]
		protected IJSRuntime JSRuntime { get; set; }

		protected override async Task OnAfterRenderAsync(bool firstRender)
		{
			await base.OnAfterRenderAsync(firstRender);
			if (firstRender)
			{
				await JSRuntime.InvokeVoidAsync("setTitle", "Volejbal - Správa osob");
			}
		}
	}
}
