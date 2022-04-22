using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using KandaEu.Volejbal.Web.Components.ProgressComponent;
using Microsoft.AspNetCore.Components;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Nastenka;

public partial class Nastenka
{
    private NovyVzkazFormData formData = new NovyVzkazFormData();
    private NastenkaState State = new NastenkaState();

    [CascadingParameter]
    public Progress Progress { get; set; }

    [Inject]
    protected Sotsera.Blazor.Toaster.IToaster Toaster { get; set; }

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

        Toaster.Success("Vzkaz zapsán.");
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
