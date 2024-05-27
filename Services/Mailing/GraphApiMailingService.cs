using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Me.SendMail;
using Microsoft.Graph.Models;
using MimeKit;

namespace KandaEu.Volejbal.Services.Mailing;

[Service]
public class GraphApiMailingService(
		[FromKeyedServices("SendMail")] GraphServiceClient _graphServiceClient,
		IOptions<MailingOptions> _mailingOptions) : IMailingService
{
	private readonly MailingOptions _mailingOptionsValue = _mailingOptions.Value;

	public Task VerifyHealthAsync(CancellationToken cancellationToken)
	{
		throw new NotImplementedException();
	}

	public async Task SendAsync(MimeMessage mailMessage, CancellationToken cancellationToken = default)
	{
		SmtpMailingService.TransformMailMessage(mailMessage, _mailingOptionsValue);

		SendMailPostRequestBody sendMailPostRequestBody = new SendMailPostRequestBody
		{
			Message = new Microsoft.Graph.Models.Message
			{
				Sender = new Recipient { EmailAddress = new EmailAddress { Name = "Kanda, Jiří [VolejbalApp]", Address = "kanda@havit.cz" } },
				ToRecipients = mailMessage.To.Cast<MailboxAddress>().Select(address => new Recipient { EmailAddress = new EmailAddress { Name = address.Name, Address = address.Address } }).ToList(),
				Subject = mailMessage.Subject,
				Body = new ItemBody
				{
					ContentType = BodyType.Text,
					Content = ((TextPart)mailMessage.Body).Text
				}
			},
			SaveToSentItems = true
		};
		await _graphServiceClient.Me.SendMail.PostAsync(sendMailPostRequestBody, cancellationToken: cancellationToken);
	}
}
