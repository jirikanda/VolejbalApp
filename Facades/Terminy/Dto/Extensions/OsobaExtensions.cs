using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Facades.Terminy.Dto.Extensions
{
	public static class OsobaExtensions
	{
		public static OsobaDto ToOsobaDto(this Osoba osoba)
		{
			return new OsobaDto
			{
				Id = osoba.Id,
				PrijmeniJmeno = osoba.PrijmeniJmeno
			};
		}
	}
}
