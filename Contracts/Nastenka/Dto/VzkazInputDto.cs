using System.ComponentModel.DataAnnotations;
using KandaEu.Volejbal.Model.Metadata;

namespace KandaEu.Volejbal.Contracts.Nastenka.Dto;

public class VzkazInputDto
{
	public int AutorId { get; set; }

	[Required]
	[MaxLength(VzkazMetadata.ZpravaMaxLength)]
	public string Zprava { get; set; }
}
