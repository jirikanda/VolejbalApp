using KandaEu.Volejbal.Web.Components.ProgressComponent;
using Microsoft.JSInterop;

namespace KandaEu.Volejbal.Web.Components.Layout;

public partial class MainLayout
{
	protected ProgressState ProgressState { get; }
	protected Progress Progress { get; }

	private bool isDark;

	public MainLayout()
	{
		ProgressState = new ProgressState();
		Progress = new Progress(ProgressState, () => InvokeAsync(StateHasChanged));
	}

	protected override async Task OnAfterRenderAsync(bool firstRender)
	{
		if (firstRender)
		{
			var theme = await JS.InvokeAsync<string>("volejbalTheme.get");
			isDark = theme == "dark";
			StateHasChanged();
		}
	}

	private async Task ToggleThemeAsync()
	{
		var newTheme = await JS.InvokeAsync<string>("volejbalTheme.toggle");
		isDark = newTheme == "dark";
	}

	public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
