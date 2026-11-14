using System.Globalization;
using System.Text;

namespace KandaEu.Volejbal.Services.Client.Platba;

/// <summary>
/// Sestavuje řetězec SPAYD (Short Payment Descriptor), tedy obsah QR kódu "QR Platba" podle standardu České bankovní asociace.
/// </summary>
public static class SpaydBuilder
{
	/// <summary>
	/// Maximální délka zprávy pro příjemce podle specifikace.
	/// </summary>
	public const int MaxMessageLength = 60;

	/// <summary>
	/// Vrátí SPAYD pro platbu v CZK na zadaný účet.
	/// </summary>
	/// <param name="iban">IBAN příjemce (mezery se odstraní).</param>
	/// <param name="message">Zpráva pro příjemce. Diakritika se odstraní, protože ji banky ve zprávě neberou spolehlivě.</param>
	/// <param name="amount">Částka v CZK. Bez ní (null) ji vyplní plátce. Zapisuje se na dvě desetinná místa (zaokrouhleno), musí být kladná.</param>
	public static string Build(string iban, string message, decimal? amount = null)
	{
		string normalizedIban = new string(iban.Where((char c) => !Char.IsWhiteSpace(c)).ToArray()).ToUpperInvariant();

		string amountItem = String.Empty;
		if (amount != null)
		{
			ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(amount.Value, 0m);
			amountItem = $"*AM:{Math.Round(amount.Value, 2, MidpointRounding.AwayFromZero).ToString("0.00", CultureInfo.InvariantCulture)}";
		}

		return $"SPD*1.0*ACC:{normalizedIban}{amountItem}*CC:CZK*MSG:{NormalizeMessage(message)}";
	}

	private static string NormalizeMessage(string message)
	{
		StringBuilder builder = new StringBuilder(message.Length);
		foreach (char c in message.Normalize(NormalizationForm.FormD))
		{
			if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
			{
				builder.Append(c);
			}
		}

		string result = builder.ToString().Normalize(NormalizationForm.FormC);
		if (result.Length > MaxMessageLength)
		{
			result = result.Substring(0, MaxMessageLength);
		}

		// hvězdička odděluje položky SPAYD, v hodnotě se musí zapsat jako %2A
		return result.Replace("*", "%2A");
	}
}
