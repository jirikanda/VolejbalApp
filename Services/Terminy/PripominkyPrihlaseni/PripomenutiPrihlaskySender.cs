using KandaEu.Volejbal.Services.Mailing;
using Microsoft.Extensions.Logging;
using MimeKit;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;

namespace KandaEu.Volejbal.Services.Terminy.PripominkyPrihlaseni;

[Service<IPripomenutiPrihlaskySender>]
public class SmtpPripomenutiPrihlaskySender(
	ILogger<SmtpPripomenutiPrihlaskySender> _logger,
	IMailingService mailingService) : IPripomenutiPrihlaskySender
{
	public async Task SendPripominkaAsync(Osoba osoba, CancellationToken cancellationToken)
	{
		if (String.IsNullOrEmpty(osoba.Email))
		{
			_logger.LogWarning("Osoba {PrijmeniJmeno} nemá vyplněn email.", osoba.PrijmeniJmeno);
			return;
		}

		using var mm = new MimeMessage();

		mm.To.Add(new MailboxAddress(osoba.PrijmeniJmeno, osoba.Email));
		mm.Subject = "Připomínka přihlášení na volejbal";
		mm.Body = new TextPart("Přihlaš se prosím na volejbal nebo vyjádři svou neúčast. https://volejbal.kanda.eu");

		try
		{
			_logger.LogInformation("Odesílám email na {email}...", osoba.Email);
			await mailingService.SendAsync(mm, cancellationToken);
			_logger.LogInformation("Email odeslán.");
		}
		catch (Exception e)
		{
			_logger.LogWarning(e, "Odeslání emailu selhalo.");
			// continue...
		}
	}
}