using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Web.Components.ProgressComponent;

namespace KandaEu.Volejbal.Web.Components.Pages.Osoby;

public partial class ZalozeniNoveOsoby
{
	[Inject]
	protected IOsobaWebApiClient OsobaWebApiClient { get; set; }

	[Inject]
	protected NavigationManager NavigationManager { get; set; }

	[CascadingParameter]
	protected Progress Progress { get; set; }

	private NovaOsobaFormData _formData = new NovaOsobaFormData();

	protected async Task ValidSubmitAsync()
	{
		OsobaInputDto novaOsoba = new OsobaInputDto()
		{
			Jmeno = _formData.Jmeno,
			Prijmeni = _formData.Prijmeni,
			Email = _formData.Email
		};

		await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.VlozOsobuAsync(novaOsoba));
		NavigationManager.NavigateTo("/");
	}
}
