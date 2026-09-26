using System.ComponentModel.DataAnnotations;
using KandaEu.Volejbal.Model.Metadata;

namespace KandaEu.Volejbal.Contracts.Nastenka.Dto;

public class VzkazInputDto
{
	[Required]
	public string AutorId { get; set; }

	[Required]
	[MaxLength(VzkazMetadata.ZpravaMaxLength)]
	public string Zprava { get; set; }
}
