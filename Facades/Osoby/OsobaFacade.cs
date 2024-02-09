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

	public async Task AktivujNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await _osobaRepository.GetObjectAsync(osobaId, cancellationToken);

		CheckNeaktivniNesmazana(osoba);

		osoba.Aktivni = true;

		_unitOfWork.AddForUpdate(osoba);
		await _unitOfWork.CommitAsync(cancellationToken);
	}

	public async Task SmazNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await _osobaRepository.GetObjectAsync(osobaId, cancellationToken);

		_unitOfWork.AddForDelete(osoba);
		await _unitOfWork.CommitAsync(cancellationToken);
	}

	private void CheckNeaktivniNesmazana(Osoba osoba)
	{
		osoba.ThrowIfDeleted();
		osoba.ThrowIfAktivni();
	}


	public async Task<OsobaListDto> GetAktivniOsobyAsync(CancellationToken cancellationToken)
	{
		return await GetOsobyByAktivniAsync(true, cancellationToken);
	}

	public async Task<OsobaListDto> GetNeaktivniOsobyAsync(CancellationToken cancellationToken)
	{
		return await GetOsobyByAktivniAsync(false, cancellationToken);
	}

	public async Task<OsobaListDto> GetOsobyByAktivniAsync(bool aktivni, CancellationToken cancellationToken)
	{
		var result = new OsobaListDto
		{
			Osoby = await _osobaDataSource.Data
				.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(GetOsobyByAktivniAsync)))
				.Where(osoba => osoba.Aktivni == aktivni)
				.OrderBy(item => item.Prijmeni).ThenBy(item => item.Jmeno)
				.Select(item => new OsobaDto
				{
					Id = item.Id,
					PrijmeniJmeno = item.PrijmeniJmeno
				})
				.ToListAsync(cancellationToken)
		};
		return result;
	}
}

