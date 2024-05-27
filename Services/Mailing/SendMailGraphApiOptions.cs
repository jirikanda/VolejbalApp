namespace KandaEu.Volejbal.Services.Mailing;

public class SendMailGraphApiOptions
{
	public const string Path = "AppSettings:SendMailGraphApi";

	public string TenantId { get; set; }
	public string ClientId { get; set; }
	public string ClientSecret { get; set; }
}
