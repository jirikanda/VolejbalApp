using System.ComponentModel.DataAnnotations;
using KandaEu.Volejbal.Model.Metadata;

namespace KandaEu.Volejbal.Contracts.Osoby.Dto;

public class OsobaInputDto
{
	[Required]
	[MaxLength(OsobaMetadata.JmenoMaxLength)]
	public string Jmeno { get; set; }

	[Required]
	[MaxLength(OsobaMetadata.PrijmeniMaxLength)]
	public string Prijmeni { get; set; }

	[Required]
	[MaxLength(OsobaMetadata.EmailMaxLength)]
	[EmailAddress]
	public string Email { get; set; }
}
