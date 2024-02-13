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

	protected NovaOsobaFormData formData = new NovaOsobaFormData();

	protected async Task ValidSubmitAsync()
	{
		OsobaInputDto novaOsoba = new OsobaInputDto()
		{
			Jmeno = formData.Jmeno,
			Prijmeni = formData.Prijmeni,
			Email = formData.Email
		};

		await Progress.ExecuteInProgressAsync(async () => await OsobaWebApiClient.VlozOsobuAsync(novaOsoba));
		//Toaster.Success($"{novaOsoba.Prijmeni} {novaOsoba.Jmeno} založen(a).");
		NavigationManager.NavigateTo("/");
	}
}
