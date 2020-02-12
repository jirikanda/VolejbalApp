using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.ProgressComponent
{
	public partial class ProgressOverlay : ComponentBase
	{
		[Parameter]
		public RenderFragment ChildContent { get; set; } 

		protected ProgressState ProgressState { get; }
		protected Progress Progress { get; }

		public ProgressOverlay()
		{			
			ProgressState = new ProgressState();
			Progress = new Progress(ProgressState, () => this.StateHasChanged());
		}
	}
}

