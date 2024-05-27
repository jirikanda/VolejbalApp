using System.Net.Mail;
using MimeKit;

namespace KandaEu.Volejbal.Services.Mailing;

public interface IMailingService
{
	Task SendAsync(MimeMessage mailMessage, CancellationToken cancellationToken = default);
	Task VerifyHealthAsync(CancellationToken cancellationToken);
}