using System.Text.RegularExpressions;

namespace KandaEu.Volejbal.Services.Client.Platba;

/// <summary>
/// Převod tuzemského čísla účtu (<c>[předčíslí-]číslo/kód banky</c>) na IBAN.
/// </summary>
public static partial class IbanBuilder
{
	[GeneratedRegex(@"^(?:(?<prefix>\d{1,6})-)?(?<number>\d{2,10})/(?<bank>\d{4})$")]
	private static partial Regex AccountNumberRegex();

	/// <summary>
	/// Vrátí IBAN českého účtu ve tvaru bez mezer, např. <c>19-2000145399/0800</c> → <c>CZ6508000000192000145399</c>.
	/// </summary>
	/// <param name="accountNumber">Číslo účtu ve tvaru <c>[předčíslí-]číslo/kód banky</c> (mezery se ignorují).</param>
	/// <exception cref="FormatException">Číslo účtu nemá očekávaný tvar.</exception>
	public static string FromCzechAccountNumber(string accountNumber)
	{
		string normalized = new string(accountNumber.Where((char c) => !Char.IsWhiteSpace(c)).ToArray());
		Match match = AccountNumberRegex().Match(normalized);
		if (!match.Success)
		{
			throw new FormatException($"Číslo účtu '{accountNumber}' nemá tvar [předčíslí-]číslo/kód banky.");
		}

		string bban = match.Groups["bank"].Value
			+ match.Groups["prefix"].Value.PadLeft(6, '0')
			+ match.Groups["number"].Value.PadLeft(10, '0');

		// kontrolní číslice: BBAN + "CZ00" (C = 12, Z = 35) jako číslo, modulo 97; počítáno průběžně, ať se nepotřebuje velké číslo
		string checkInput = bban + "123500";
		int remainder = 0;
		foreach (char digit in checkInput)
		{
			remainder = ((remainder * 10) + (digit - '0')) % 97;
		}

		return $"CZ{98 - remainder:00}{bban}";
	}
}
