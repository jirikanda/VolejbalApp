using KandaEu.Volejbal.Contracts.Osoby.Dto;
using KandaEu.Volejbal.Model;

namespace KandaEu.Volejbal.Facades.Terminy.Dto.Extensions;

public static class PrihlaskaExtensions
{
    public static OsobaDto ToOsobaDto(this Prihlaska prihlaska)
    {
        return prihlaska.Osoba.ToOsobaDto();
    }
}
