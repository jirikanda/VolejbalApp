using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;

namespace KandaEu.Volejbal.Facades.Osoby;

[Service]
public class OsobaFacade(
	IOsobaRepository _osobaRepository,
	IOsobaDataSource _osobaDataSource,
	IUnitOfWork _unitOfWork) : IOsobaFacade
{
	public async Task VlozOsobuAsync(OsobaInputDto osobaInputDto, CancellationToken cancellationToken)
	{
		Osoba osoba = new Osoba
		{
			Jmeno = osobaInputDto.Jmeno,
			Prijmeni = osobaInputDto.Prijmeni,
			Email = osobaInputDto.Email
		};

		_unitOfWork.AddForInsert(osoba);
		await _unitOfWork.CommitAsync(cancellationToken);
	}

	public async Task AktivujOsobuAsync(int osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await _osobaRepository.GetObjectAsync(osobaId, cancellationToken);

		osoba.ThrowIfDeleted();
		osoba.ThrowIfAktivni();

		osoba.Aktivni = true;

		_unitOfWork.AddForUpdate(osoba);
		await _unitOfWork.CommitAsync(cancellationToken);
	}

	public async Task DeaktivujOsobuAsync(int osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await _osobaRepository.GetObjectAsync(osobaId, cancellationToken);

		osoba.ThrowIfDeleted();
		osoba.ThrowIfNotAktivni();

		osoba.Aktivni = false;

		_unitOfWork.AddForUpdate(osoba);
		await _unitOfWork.CommitAsync(cancellationToken);
	}

	/// <summary>
	/// Smaže osobu. Mazat lze jen osobu, která je již deaktivovaná — aby smazání nebylo jednokrokové.
	/// </summary>
	public async Task SmazOsobuAsync(int osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await _osobaRepository.GetObjectAsync(osobaId, cancellationToken);

		osoba.ThrowIfDeleted();
		osoba.ThrowIfAktivni();

		_unitOfWork.AddForDelete(osoba);
		await _unitOfWork.CommitAsync(cancellationToken);
	}

	/// <summary>
	/// Všechny nesmazané osoby, aktivní i neaktivní (pro obrazovku správy hráčů).
	/// </summary>
	public async Task<OsobaListDto> GetOsobyAsync(CancellationToken cancellationToken)
	{
		return await GetOsobyAsync(null, cancellationToken);
	}

	public async Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken)
	{
		return await GetOsobyAsync(true, cancellationToken);
	}

	private async Task<OsobaListDto> GetOsobyAsync(bool? aktivni, CancellationToken cancellationToken)
	{
		OsobaListDto result = new OsobaListDto
		{
			Osoby = await _osobaDataSource.Data
				.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetOsobyAsync)))
				.Where(osoba => (aktivni == null) || (osoba.Aktivni == aktivni.Value))
				.OrderBy(item => item.Prijmeni).ThenBy(item => item.Jmeno)
				.Select(item => new OsobaDto
				{
					Id = item.Id,
					PrijmeniJmeno = item.PrijmeniJmeno,
					Aktivni = item.Aktivni
				})
				.ToListAsync(cancellationToken)
		};
		return result;
	}
}
