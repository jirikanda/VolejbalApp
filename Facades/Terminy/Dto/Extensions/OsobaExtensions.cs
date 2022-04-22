using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Facades.Terminy.Dto.Extensions;

public static class OsobaExtensions
{
    public static OsobaDto ToOsobaDto(this Osoba osoba)
    {
        return new OsobaDto
        {
            Id = osoba.Id,
            PrijmeniJmeno = osoba.PrijmeniJmeno
        };
    }
}
