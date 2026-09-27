namespace KandaEu.Volejbal.Services.Terminy.EnsureTerminy;

/// <summary>
/// Výpočet dat termínů k založení. Bez závislostí a s datem předaným parametrem, aby šel testovat v CI
/// bez databáze a bez DI.
/// </summary>
/// <remarks>
/// Termíny navazují na poslední existující termín (stejný den v týdnu, +7 dní), přeskakují letní
/// prázdniny, vánoční prázdniny a státní svátky spadající do školního roku. Deterministický výpočet je
/// podstatný pro souběh v <see cref="EnsureTerminyService" />: souběžné běhy dostanou totéž datum,
/// a tedy soupeří o tentýž dokument.
/// </remarks>
[Service]
public class TerminDatumGeneratorService : ITerminDatumGeneratorService
{
	/// <summary>
	/// Data termínů, které je potřeba založit, aby bylo k dispozici <paramref name="pozadovanyPocet" />
	/// budoucích termínů.
	/// </summary>
	/// <param name="today">Dnešní datum.</param>
	/// <param name="posledniDatum">Datum posledního existujícího termínu (včetně smazaných), null pokud žádný není.</param>
	/// <param name="pocetBudoucichTerminu">Kolik budoucích termínů už existuje.</param>
	/// <param name="pozadovanyPocet">Kolik budoucích termínů má být k dispozici.</param>
	public List<DateTime> GetDatumyKZalozeni(DateTime today, DateTime? posledniDatum, int pocetBudoucichTerminu, int pozadovanyPocet)
	{
		List<DateTime> result = new List<DateTime>();
		today = today.Date;

		DateTime datum;
		if (posledniDatum == null)
		{
			// Žádný termín zatím není: první úterý ode dneška.
			datum = today;
			while (datum.DayOfWeek != DayOfWeek.Tuesday)
			{
				datum = datum.AddDays(1);
			}
		}
		else
		{
			// Navazujeme na poslední termín: stejný den následující týden...
			datum = posledniDatum.Value.Date.AddDays(7);

			// ...a kdyby poslední termín ležel hluboko v minulosti (aplikace se dlouho nepoužívala),
			// posuneme se po týdnech do budoucnosti.
			while (datum < today)
			{
				datum = datum.AddDays(7);
			}
		}

		for (int i = pocetBudoucichTerminu; i < pozadovanyPocet; i++)
		{
			while (!IsSchoolDate(datum))
			{
				datum = datum.AddDays(7);
			}

			result.Add(datum);
			datum = datum.AddDays(7);
		}

		return result;
	}

	private static bool IsSchoolDate(DateTime datum)
	{
		return !IsSummerHoliday(datum)
			&& !IsChristmasHoliday(datum)
			&& !IsHolidayDate(datum, 1, 5) // Svátek práce
			&& !IsHolidayDate(datum, 8, 5) // Den vítězství
			&& !IsHolidayDate(datum, 28, 9) // Den české státnosti
			&& !IsHolidayDate(datum, 28, 10) // Den vzniku Československa
			&& !IsHolidayDate(datum, 17, 11); // Den boje za svobodu a demokracii
	}

	private static bool IsSummerHoliday(DateTime datum)
	{
		return datum.Month is 7 or 8;
	}

	private static bool IsChristmasHoliday(DateTime datum)
	{
		return ((datum.Month == 12) && (datum.Day >= 24))
			|| ((datum.Month == 1) && (datum.Day == 1));
	}

	private static bool IsHolidayDate(DateTime datum, int day, int month)
	{
		return (datum.Day == day) && (datum.Month == month);
	}
}
