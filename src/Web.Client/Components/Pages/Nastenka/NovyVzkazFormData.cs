using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using System.ComponentModel.DataAnnotations;

namespace KandaEu.Volejbal.Web.Client.Components.Pages.Nastenka;

public class NovyVzkazFormData
{
	/// <remarks>
	/// Prázdná volba v InputSelectu má hodnotu "", což [Required] nad stringem zachytí stejně
	/// jako dřív null nad int?.
	/// </remarks>
	[Required(ErrorMessage = "Zadej, kdo zprávu posílá.")]
	public string AutorId { get; set; }

	[Required(ErrorMessage = "Zadej zprávu.")]
	public string Zprava { get; set; }

	public VzkazInputDto ToVzkazInputDto()
	{
		return new VzkazInputDto
		{
			AutorId = this.AutorId,
			Zprava = this.Zprava
		};
	}

	public override string ToString()
	{
		return $"[AuthorID: {AutorId}, Zprava: {Zprava}]";
	}
}
