using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace KandaEu.Volejbal.Services.Mailing;

[Service<IMailingService>]
public class SmtpMailingService(
		IOptions<MailingOptions> _mailingOptions) : IMailingService
{
	private readonly MailingOptions _mailingOptionsValue = _mailingOptions.Value;

	public async Task VerifyHealthAsync(CancellationToken cancellationToken)
	{
		using SmtpClient smtpClient = await CreateConnectedSmtpClientAsync(cancellationToken);
	}

	public void Send(MimeMessage mailMessage)
	{
		using SmtpClient smtpClient = CreateConnectedSmtpClient();
		TransformMailMessage(mailMessage, _mailingOptionsValue);
		smtpClient.Send(mailMessage);
	}

	public async Task SendAsync(MimeMessage mailMessage, CancellationToken cancellationToken = default)
	{
		using SmtpClient smtpClient = await CreateConnectedSmtpClientAsync(cancellationToken);
		TransformMailMessage(mailMessage, _mailingOptionsValue);
		await smtpClient.SendAsync(mailMessage, cancellationToken);
	}

	internal static void TransformMailMessage(MimeMessage mailMessage, MailingOptions mailingOptions)
	{
		if (!mailMessage.From.Any())
		{
			mailMessage.From.Add(InternetAddress.Parse(mailingOptions.From));
		}

		if (!String.IsNullOrEmpty(mailingOptions.OverrideTo))
		{
			mailMessage.To.Clear();
			mailMessage.Cc.Clear();
			mailMessage.Bcc.Clear();
			System.Diagnostics.Debug.Assert(mailMessage.To.Count == 0);
			System.Diagnostics.Debug.Assert(mailMessage.Cc.Count == 0);
			System.Diagnostics.Debug.Assert(mailMessage.Bcc.Count == 0);

			mailMessage.To.AddRange(InternetAddressList.Parse(mailingOptions.OverrideTo));
		}

		// doručování html mailu do gmailu pravděpodobně nemá rádo 8bit
		mailMessage.Prepare(EncodingConstraint.SevenBit);
	}

	private async Task<SmtpClient> CreateConnectedSmtpClientAsync(CancellationToken cancellationToken)
	{
		var smtpClient = new SmtpClient();
		await smtpClient.ConnectAsync(_mailingOptionsValue.SmtpServer, _mailingOptionsValue.SmtpPort ?? 0, _mailingOptionsValue.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, cancellationToken);

		if (_mailingOptionsValue.HasCredentials())
		{
			await smtpClient.AuthenticateAsync(_mailingOptionsValue.SmtpUsername, _mailingOptionsValue.SmtpPassword, cancellationToken);
		}

		return smtpClient;
	}

	private SmtpClient CreateConnectedSmtpClient()
	{
		var smtpClient = new SmtpClient();
		smtpClient.Connect(_mailingOptionsValue.SmtpServer, _mailingOptionsValue.SmtpPort ?? 0, _mailingOptionsValue.UseSsl);

		if (_mailingOptionsValue.HasCredentials())
		{
			smtpClient.Authenticate(_mailingOptionsValue.SmtpUsername, _mailingOptionsValue.SmtpPassword);
		}

		return smtpClient;
	}
}