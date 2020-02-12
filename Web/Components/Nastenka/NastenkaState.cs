using KandaEu.Volejbal.Web.WebApiClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Nastenka
{
	public class NastenkaState
	{
		public List<VzkazDto> Vzkazy { get; set; }
		public List<OsobaDto2> AktivniOsoby { get; set; }
	}
}
