using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Nastenka
{
	public class NastenkaFormData
	{
		[Required(ErrorMessage = "Zadej, kdo zprávu posílá.")]
		public int? AutorId { get; set; }

		[Required(ErrorMessage = "Zadej zprávu.")]
		public string Zprava { get; set; }

		// TODO: Reuse Contractů.
		//public VzkazInputDto ToVzkazInputDto()
		//{
		//	return new VzkazInputDto
		//	{
		//		AutorId = this.AutorId.Value,
		//		Zprava = this.Zprava
		//	};
		//}
	}
}
