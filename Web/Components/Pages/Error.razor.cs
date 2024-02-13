namespace KandaEu.Volejbal.Web.Components.Pages;

public partial class Error
{
	[Inject] public NavigationManager NavigationManager { get; set; }

	private void HandleRestartClick()
	{
		NavigationManager.NavigateTo("/", forceLoad: true);
	}
}
