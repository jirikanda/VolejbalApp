using Havit.Data.EntityFrameworkCore;
using Havit.Services.TimeServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

[Service]
public class EnsureTerminyService(
	ILogger<EnsureTerminyService> _logger,
	ITerminDataSource _terminDataSource,
	IUnitOfWork _unitOfWork,
	ITimeService _timeService) : IEnsureTerminyService
{
	public async Task EnsureTerminyAsync(CancellationToken cancellationToken)
	{
		_logger.LogInformation("Zjišťuji počet budoucích termínů...");
		int budouciTerminyPocet = await _terminDataSource.Data
			.TagWith(QueryTagBuilder.CreateTag(this.GetType(), nameof(EnsureTerminyAsync)))
			.Where(termin => termin.Datum.Date >= _timeService.GetCurrentDate())
			.CountAsync(cancellationToken);

		_logger.LogInformation("Nalezeno {count} budoucích termínů.", budouciTerminyPocet);

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
				while (!IsSchoolDate(datum))
				{
					datum = datum.AddDays(7);
				}

				_logger.LogInformation("Zakládám termín pro datum {datum}.", datum);
				Termin termin = new Termin { Datum = datum };
				_unitOfWork.AddForInsert(termin);
				datum = datum.AddDays(7);
			}

			_logger.LogInformation("Ukládám...");
			await _unitOfWork.CommitAsync(cancellationToken);
			_logger.LogInformation("Uloženo...");
		}
	}

	private bool IsSchoolDate(DateTime datum)
	{
		return !IsSummerHoliday(datum)
			&& !IsChristmasHoliday(datum)
			&& !IsHolidayDate(datum, 1, 5) // Svátek práce
			&& !IsHolidayDate(datum, 8, 5) // Den vítězství
			&& !IsHolidayDate(datum, 28, 9) // Den české státnosti
			&& !IsHolidayDate(datum, 28, 10) // Den vzniku Československa
			&& !IsHolidayDate(datum, 17, 11); // Den boje za svobodu a demokracii
	}

	private bool IsSummerHoliday(DateTime datum)
	{
		return datum.Month is 7 or 8;
	}

	private bool IsChristmasHoliday(DateTime datum)
	{
		return ((datum.Month == 12) && (datum.Day >= 24))
			|| ((datum.Month == 1) && (datum.Day == 1));
	}

	private bool IsHolidayDate(DateTime datum, int day, int month) => (datum.Day == day) && (datum.Month == month);
}
