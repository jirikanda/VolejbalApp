namespace KandaEu.Volejbal.Web.Client.Components.Pages;

public partial class Error
{
	[Inject] public NavigationManager NavigationManager { get; set; }

	private void HandleRestartClick()
	{
		NavigationManager.NavigateTo("/", forceLoad: true);
	}
}
