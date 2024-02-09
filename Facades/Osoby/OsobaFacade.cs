using Havit.Data.EntityFrameworkCore;
using KandaEu.Volejbal.Contracts.Osoby;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using Microsoft.EntityFrameworkCore;

namespace KandaEu.Volejbal.Facades.Osoby;

[Service]
public class OsobaFacade : IOsobaFacade
{
	private readonly IOsobaRepository osobaRepository;
	private readonly IOsobaDataSource osobaDataSource;
	private readonly IUnitOfWork unitOfWork;

	public OsobaFacade(IOsobaRepository osobaRepository, IOsobaDataSource osobaDataSource, IUnitOfWork unitOfWork)
	{
		this.osobaRepository = osobaRepository;
		this.osobaDataSource = osobaDataSource;
		this.unitOfWork = unitOfWork;
	}

	public async Task VlozOsobuAsync(OsobaInputDto osobaInputDto, CancellationToken cancellationToken)
	{
		Osoba osoba = new Osoba
		{
			Jmeno = osobaInputDto.Jmeno,
			Prijmeni = osobaInputDto.Prijmeni,
			Email = osobaInputDto.Email
		};

		unitOfWork.AddForInsert(osoba);
		await unitOfWork.CommitAsync(cancellationToken);
	}

	public async Task AktivujNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await osobaRepository.GetObjectAsync(osobaId, cancellationToken);

		CheckNeaktivniNesmazana(osoba);

		osoba.Aktivni = true;

		unitOfWork.AddForUpdate(osoba);
		await unitOfWork.CommitAsync(cancellationToken);
	}

	public async Task SmazNeaktivniOsobuAsync(int osobaId, CancellationToken cancellationToken)
	{
		Osoba osoba = await osobaRepository.GetObjectAsync(osobaId, cancellationToken);

		unitOfWork.AddForDelete(osoba);
		await unitOfWork.CommitAsync(cancellationToken);
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
			Osoby = await osobaDataSource.Data
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

