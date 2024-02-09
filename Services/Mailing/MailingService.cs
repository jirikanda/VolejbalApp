using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;

namespace KandaEu.Volejbal.Services.Mailing;

[Service]
public class MailingService(IOptions<MailingOptions> _mailingOptions) : IMailingService
{
	private MailingOptions _mailingOptionsValue = _mailingOptions.Value;

	public void Send(MailMessage mailMessage)
	{
		using (SmtpClient smtpClient = new SmtpClient())
		{
			smtpClient.Host = _mailingOptionsValue.SmtpServer;
			if (_mailingOptionsValue.SmtpPort != null)
			{
				smtpClient.Port = _mailingOptionsValue.SmtpPort.Value;
			}
			smtpClient.EnableSsl = _mailingOptionsValue.UseSsl;
			if (_mailingOptionsValue.HasCredentials())
			{
				smtpClient.Credentials = new NetworkCredential(_mailingOptionsValue.SmtpUsername, _mailingOptionsValue.SmtpPassword);
			}

			if ((mailMessage.From == null)
				|| String.IsNullOrWhiteSpace(mailMessage.From.Address))
			{
				mailMessage.From = new MailAddress(_mailingOptionsValue.From);
			}

			smtpClient.Send(mailMessage);
		}
	}
}
