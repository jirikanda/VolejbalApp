using KandaEu.Volejbal.Web.Client.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Nastenka;

public partial class NastenkaSidebar
{
	private NovyVzkazFormData _formData = new NovyVzkazFormData();
	private NastenkaState _state = new NastenkaState();

	[CascadingParameter]
	protected Progress Progress { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		await LoadDataAsync();
	}

	private async Task OnValidSubmitAsync()
	{
		await NastenkaWebApiClient.VlozVzkazAsync(_formData.ToVzkazInputDto());
		_formData.Zprava = "";
		await LoadDataAsync();
	}

	private async Task LoadDataAsync()
	{
		_state.AktivniOsoby = null;
		_state.Vzkazy = null;

		await Progress.ExecuteInProgressAsync(async () =>
		{
			_state.AktivniOsoby = (await OsobaWebApiClient.GetAktivniOsobyAsync()).Osoby.ToList();
			_state.Vzkazy = (await NastenkaWebApiClient.GetVzkazyAsync()).Vzkazy.ToList();
		});
	}
}
