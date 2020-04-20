using KandaEu.Volejbal.Contracts.Nastenka.Dto;
using KandaEu.Volejbal.Contracts.Osoby.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Nastenka
{
	public class NastenkaState
	{
		public List<VzkazDto> Vzkazy { get; set; }
		public List<OsobaDto> AktivniOsoby { get; set; }
	}
}
