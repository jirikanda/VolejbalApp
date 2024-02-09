using Havit.Data.EntityFrameworkCore;
using Havit.Services.TimeServices;
using Microsoft.EntityFrameworkCore;

namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

[Service]
public class EnsureTerminyService(
	ITerminDataSource _terminDataSource,
	IUnitOfWork _unitOfWork,
	ITimeService _timeService) : IEnsureTerminyService
{
	public async Task EnsureTerminyAsync(CancellationToken cancellationToken)
	{
		int budouciTerminyPocet = await _terminDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(EnsureTerminyAsync)))
			.Where(termin => termin.Datum.Date >= _timeService.GetCurrentDate())
			.CountAsync(cancellationToken);

		if (budouciTerminyPocet < 3)
		{
			DateTime posledniDatum = await _terminDataSource.DataIncludingDeleted
				.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(EnsureTerminyAsync)))
				.OrderByDescending(item => item.Datum)
				.Select(item => item.Datum)
				.FirstOrDefaultAsync(cancellationToken);

			DateTime datum;
			if (posledniDatum == default(DateTime))
			{
				// pokud není žádné datum, vezmeme první úterý ode dneška
				datum = _timeService.GetCurrentDate();
				while (datum.DayOfWeek != DayOfWeek.Tuesday)
				{
					datum = datum.AddDays(1);
				}
			}
			else
			{
				// pokud již nějaké datum máme, přidáme vezmeme stejný den následující týden
				datum = posledniDatum.AddDays(7);

				while (datum < _timeService.GetCurrentDate()) // pokud by to náhodou bylo v minulosti, což se nečeká, posuneme se do budoucnosti
				{
					datum = datum.AddDays(7); // k tomuto snad nikdy nedojde
				}
			}

			for (int i = budouciTerminyPocet; i < 3; i++)
			{
				Termin termin = new Termin { Datum = datum };
				_unitOfWork.AddForInsert(termin);
				datum = datum.AddDays(7);
			}

			await _unitOfWork.CommitAsync(cancellationToken);
		}
	}
}
