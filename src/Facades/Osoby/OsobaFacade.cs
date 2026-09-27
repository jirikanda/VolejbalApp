using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Facades.Osoby;

[Service(ServiceType = typeof(IOsobaApi))]
public class OsobaFacade(
	IOsobaRepository _osobaRepository,
	TimeProvider _timeProvider) : IOsobaApi
{
	public async Task VlozOsobuAsync(OsobaInputDto osobaInputDto, CancellationToken cancellationToken)
	{
		Osoba osoba = new Osoba
		{
			Jmeno = osobaInputDto.Jmeno,
			Prijmeni = osobaInputDto.Prijmeni,
			Email = osobaInputDto.Email
		};

		await _osobaRepository.InsertAsync(osoba, cancellationToken);
	}

	public async Task AktivujOsobuAsync(string osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await _osobaRepository.GetOsobaAsync(osobaId, cancellationToken);

		osoba.ThrowIfDeleted();
		osoba.ThrowIfAktivni();

		osoba.Aktivni = true;

		await _osobaRepository.UpdateAsync(osoba, cancellationToken);
	}

	public async Task DeaktivujOsobuAsync(string osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await _osobaRepository.GetOsobaAsync(osobaId, cancellationToken);

		osoba.ThrowIfDeleted();
		osoba.ThrowIfNotAktivni();

		osoba.Aktivni = false;

		await _osobaRepository.UpdateAsync(osoba, cancellationToken);
	}

	/// <summary>
	/// Smaže osobu. Mazat lze jen osobu, která je již deaktivovaná — aby smazání nebylo jednokrokové.
	/// </summary>
	/// <remarks>
	/// Soft delete: osoba zůstává dokumentem s vyplněným deleted. Přihlášky smazané osoby odkazují
	/// na id, které se pak nenajde mezi načtenými osobami, a detail termínu je přeskočí.
	/// </remarks>
	public async Task SmazOsobuAsync(string osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await _osobaRepository.GetOsobaAsync(osobaId, cancellationToken);

		osoba.ThrowIfDeleted();
		osoba.ThrowIfAktivni();

		osoba.Deleted = _timeProvider.GetLocalDateTime();

		await _osobaRepository.UpdateAsync(osoba, cancellationToken);
	}

	/// <summary>
	/// Všechny nesmazané osoby, aktivní i neaktivní (pro obrazovku správy hráčů).
	/// </summary>
	public async Task<OsobaListDto> GetOsobyAsync(CancellationToken cancellationToken)
	{
		return ToOsobaListDto(await _osobaRepository.GetAllAsync(cancellationToken));
	}

	public async Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken)
	{
		return ToOsobaListDto(await _osobaRepository.GetAllAktivniAsync(cancellationToken));
	}

	private static OsobaListDto ToOsobaListDto(List<Osoba> osoby)
	{
		// Řazení podle příjmení a jména zajišťuje repozitář (české řazení, viz OsobaRazeniExtensions).
		return new OsobaListDto
		{
			Osoby = osoby
				.Select(osoba => new OsobaDto
				{
					Id = osoba.Id,
					PrijmeniJmeno = osoba.PrijmeniJmeno,
					Aktivni = osoba.Aktivni
				})
				.ToList()
		};
	}
}
