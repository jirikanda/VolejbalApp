namespace KandaEu.Volejbal.Web.Components.ProgressComponent;

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

