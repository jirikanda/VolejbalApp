using System.Globalization;
using KandaEu.Volejbal.Services.Client.Platba;
using Microsoft.Extensions.Configuration;
using Net.Codecrete.QrCodeGenerator;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Platba;

/// <summary>
/// QR kód pro platbu (QR Platba), stejný pro všechny termíny. Bez nastaveného účtu (<c>PlatbaUcet</c>,
/// tvar <c>[předčíslí-]číslo/kód banky</c>) se nezobrazí nic, částka (<c>PlatbaCastka</c>, v CZK) je volitelná.
/// </summary>
public partial class PlatbaQr : ComponentBase
{
	private const string Message = "Volejbal";

	[Inject]
	protected IConfiguration Configuration { get; set; }

	private string _qrSvg;
	private string _ucet;
	private string _castkaText;

	protected override void OnInitialized()
	{
		string ucet = Configuration["PlatbaUcet"];
		if (String.IsNullOrWhiteSpace(ucet))
		{
			return;
		}

		// částka je volitelná, bez ní (nebo s neplatnou hodnotou) ji vyplní plátce
		decimal? castka = (Decimal.TryParse(Configuration["PlatbaCastka"], NumberStyles.Number, CultureInfo.InvariantCulture, out decimal parsedCastka) && (parsedCastka > 0))
			? parsedCastka
			: null;
		string spayd = SpaydBuilder.Build(IbanBuilder.FromCzechAccountNumber(ucet), Message, castka);

		_ucet = ucet.Trim();
		_castkaText = (castka == null)
			? null
			: $"{castka.Value.ToString("0.##", CultureInfo.CurrentCulture)} Kč";

		// tmavé moduly na bílém pozadí i v tmavém režimu, jinak by QR kód banky nenačetly
		_qrSvg = QrCode.EncodeText(spayd, QrCode.Ecc.Medium).ToSvgString(2, "#000000", "#ffffff");
	}
}
