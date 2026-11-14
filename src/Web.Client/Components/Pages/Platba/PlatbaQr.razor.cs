using System.Globalization;
using KandaEu.Volejbal.Services.Client.Platba;
using Microsoft.Extensions.Configuration;
using Net.Codecrete.QrCodeGenerator;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Platba;

/// <summary>
/// QR kód pro platbu (QR Platba) za zvolený termín. Zpráva pro příjemce nese datum termínu,
/// takže se s termínem mění. Bez nastaveného účtu (<c>PlatbaIban</c>) se nezobrazí nic,
/// částka (<c>PlatbaCastka</c>, v CZK) je volitelná.
/// </summary>
public partial class PlatbaQr : ComponentBase
{
	[Inject]
	protected IConfiguration Configuration { get; set; }

	/// <summary>
	/// Id termínu, tedy jeho datum ve tvaru yyyy-MM-dd.
	/// </summary>
	[Parameter] public string TerminId { get; set; }

	private string _qrSvg;
	private string _message;
	private string _renderedTerminId;

	protected override void OnParametersSet()
	{
		if (TerminId == _renderedTerminId)
		{
			return;
		}
		_renderedTerminId = TerminId;

		string iban = Configuration["PlatbaIban"];
		if (String.IsNullOrWhiteSpace(iban)
			|| !DateTime.TryParseExact(TerminId, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime datum))
		{
			_qrSvg = null;
			_message = null;
			return;
		}

		string message = $"Volejbal, termín {datum:d.M.yyyy}";
		// částka je volitelná, bez ní (nebo s neplatnou hodnotou) ji vyplní plátce
		decimal? castka = (Decimal.TryParse(Configuration["PlatbaCastka"], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedCastka) && (parsedCastka > 0))
			? parsedCastka
			: null;
		string spayd = SpaydBuilder.Build(iban, message, castka);

		// popisek pod QR kódem: zpráva pro příjemce, a pokud máme částku, tak i ta (částka není součástí zprávy v QR)
		_message = (castka == null)
			? message
			: $"{message}, {castka.Value.ToString("0.##", CultureInfo.CurrentCulture)} Kč";

		// tmavé moduly na bílém pozadí i v tmavém režimu, jinak by QR kód banky nenačetly
		_qrSvg = QrCode.EncodeText(spayd, QrCode.Ecc.Medium).ToSvgString(2, "#000000", "#ffffff");
	}
}
