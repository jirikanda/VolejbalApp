using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using System.ComponentModel.DataAnnotations;

namespace KandaEu.Volejbal.Web.Components.Nastenka;

public class NovyVzkazFormData
{
	[Required(ErrorMessage = "Zadej, kdo zprávu posílá.")]
	public int? AutorId { get; set; }

	[Required(ErrorMessage = "Zadej zprávu.")]
	public string Zprava { get; set; }

	public VzkazInputDto ToVzkazInputDto()
	{
		return new VzkazInputDto
		{
			AutorId = this.AutorId.Value,
			Zprava = this.Zprava
		};
	}

	public override string ToString()
	{
		return $"[AuthorID: {AutorId}, Zprava: {Zprava}]";
	}
}
