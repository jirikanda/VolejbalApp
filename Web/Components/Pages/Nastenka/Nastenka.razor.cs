using KandaEu.Volejbal.Web.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Components.Pages.Nastenka;

public partial class Nastenka
{
	private NovyVzkazFormData formData = new NovyVzkazFormData();
	private NastenkaState State = new NastenkaState();

	[CascadingParameter]
	public Progress Progress { get; set; }

	protected override async Task OnInitializedAsync()
	{
		await base.OnInitializedAsync();
		await LoadDataAsync();
	}

	private async Task OnValidSubmitAsync()
	{
		await NastenkaWebApiClient.VlozVzkazAsync(formData.ToVzkazInputDto());
		formData.Zprava = ""; // vyčistit formulář
		await LoadDataAsync();

		//Toaster.Success("Vzkaz zapsán.");
	}

	private async Task LoadDataAsync()
	{
		State.AktivniOsoby = null;
		State.Vzkazy = null;

		await Progress.ExecuteInProgressAsync(async () =>
		{
			State.AktivniOsoby = (await OsobaWebApiClient.GetAktivniOsobyAsync()).Osoby.ToList();
			State.Vzkazy = (await NastenkaWebApiClient.GetVzkazyAsync()).Vzkazy.ToList();
		});
	}
}
