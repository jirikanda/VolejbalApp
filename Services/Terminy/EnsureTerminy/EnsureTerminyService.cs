using Havit.Services.TimeServices;

namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

[Service]
public class EnsureTerminyService : IEnsureTerminyService
{
	private readonly ITerminDataSource terminDataSource;
	private readonly IUnitOfWork unitOfWork;
	private readonly ITimeService timeService;

	public EnsureTerminyService(ITerminDataSource terminDataSource, IUnitOfWork unitOfWork, ITimeService timeService)
	{
		this.terminDataSource = terminDataSource;
		this.unitOfWork = unitOfWork;
		this.timeService = timeService;
	}

	public void EnsureTerminy()
	{
		lock (typeof(EnsureTerminyService))
		{
			int budouciTerminyPocet = terminDataSource.Data.Where(termin => termin.Datum.Date >= timeService.GetCurrentDate()).Count();

			if (budouciTerminyPocet < 3)
			{
				DateTime posledniDatum = terminDataSource.DataIncludingDeleted.OrderByDescending(item => item.Datum).Select(item => item.Datum).FirstOrDefault();

				DateTime datum;
				if (posledniDatum == default(DateTime))
				{
					// pokud není žádné datum, vezmeme první úterý ode dneška
					datum = timeService.GetCurrentDate();
					while (datum.DayOfWeek != DayOfWeek.Tuesday)
					{
						datum = datum.AddDays(1);
					}
				}
				else
				{
					// pokud již nějaké datum máme, přidáme vezmeme stejný den následující týden
					datum = posledniDatum.AddDays(7);

					while (datum < timeService.GetCurrentDate()) // pokud by to náhodou bylo v minulosti, což se nečeká, posuneme se do budoucnosti
					{
						datum = datum.AddDays(7); // k tomuto snad nikdy nedojde
					}
				}

				for (int i = budouciTerminyPocet; i < 3; i++)
				{
					Termin termin = new Termin { Datum = datum };
					unitOfWork.AddForInsert(termin);
					datum = datum.AddDays(7);
				}

				unitOfWork.Commit();
			}
		}
	}
}
