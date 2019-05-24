using KandaEu.Volejbal.Contracts.Terminy.Dto;
using KandaEu.Volejbal.Model;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Facades.Terminy.Dto.Extensions
{
	public static class PrihlaskaExtensions
	{
		public static OsobaDto ToOsobaDto(this Prihlaska prihlaska)
		{
			return prihlaska.Osoba.ToOsobaDto();
		}
	}
}
