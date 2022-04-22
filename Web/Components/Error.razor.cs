using Microsoft.AspNetCore.Components;

namespace KandaEu.Volejbal.Web.Components;

public partial class Error
{
    [Inject] public NavigationManager NavigationManager { get; set; }

    private void HandleRestartClick()
    {
        NavigationManager.NavigateTo("/", forceLoad: true);
    }
}
