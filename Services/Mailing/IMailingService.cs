using System.Net.Mail;

namespace KandaEu.Volejbal.Services.Mailing;

public interface IMailingService
{
    void Send(MailMessage mailMessage);
}
