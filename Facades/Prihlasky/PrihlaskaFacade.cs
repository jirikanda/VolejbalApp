using Havit.Services.TimeServices;
using KandaEu.Volejbal.Contracts.Prihlasky;

namespace KandaEu.Volejbal.Facades.Prihlasky;

[Service]
public class PrihlaskaFacade(
	IUnitOfWork _unitOfWork,
	ITimeService _timeService,
	IPrihlaskaRepository _prihlaskaRepository) : IPrihlaskaFacade
{
	//private static object _lock = new object();

	public async Task PrihlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken)
	{
		// TODO: lock (_lock)
		{
			if (await _prihlaskaRepository.GetPrihlaskaAsync(terminId, osobaId, cancellationToken) == null)
			{
				Prihlaska prihlaska = new Prihlaska
				{
					TerminId = terminId,
					OsobaId = osobaId,
					DatumPrihlaseni = _timeService.GetCurrentTime(),
				};

				_unitOfWork.AddForInsert(prihlaska);
				await _unitOfWork.CommitAsync(cancellationToken);
			}
		}
	}

	public async Task OdhlasitAsync(int terminId, int osobaId, CancellationToken cancellationToken)
	{
		// TODO: lock (_lock)
		{
			Prihlaska prihlaska = await _prihlaskaRepository.GetPrihlaskaAsync(terminId, osobaId, cancellationToken);
			if (prihlaska != null)
			{
				_unitOfWork.AddForDelete(prihlaska);
			}
			else
			{
				var now = _timeService.GetCurrentTime();

				_unitOfWork.AddForInsert(new Prihlaska
				{
					TerminId = terminId,
					OsobaId = osobaId,
					DatumPrihlaseni = now,
					Deleted = now
				});
			}
			await _unitOfWork.CommitAsync(cancellationToken);
		}
	}
}
