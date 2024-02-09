using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Prihlasky;

namespace KandaEu.Volejbal.Facades.Prihlasky;

[Service]
public class PrihlaskaFacade : IPrihlaskaFacade
{
	private readonly IUnitOfWork unitOfWork;
	private readonly ITimeService timeService;
	private readonly IPrihlaskaRepository prihlaskaRepository;

	private static object _lock = new object();

	public PrihlaskaFacade(IUnitOfWork unitOfWork, ITimeService timeService, IPrihlaskaRepository prihlaskaRepository)
	{
		this.unitOfWork = unitOfWork;
		this.timeService = timeService;
		this.prihlaskaRepository = prihlaskaRepository;
	}

	public async Task PrihlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken)
	{
		// TODO: lock (_lock)
		{
			if (await prihlaskaRepository.GetPrihlaskaAsync(terminId, osobaId, cancellationToken) == null)
			{
				Prihlaska prihlaska = new Prihlaska
				{
					TerminId = terminId,
					OsobaId = osobaId,
					DatumPrihlaseni = timeService.GetCurrentTime(),
				};

				unitOfWork.AddForInsert(prihlaska);
				await unitOfWork.CommitAsync(cancellationToken);
			}
		}
	}

	public async Task OdhlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken)
	{
		// TODO: lock (_lock)
		{
			Prihlaska prihlaska = await prihlaskaRepository.GetPrihlaskaAsync(terminId, osobaId, cancellationToken);
			if (prihlaska != null)
			{
				unitOfWork.AddForDelete(prihlaska);
				await unitOfWork.CommitAsync(cancellationToken);
			}
		}
	}
}
