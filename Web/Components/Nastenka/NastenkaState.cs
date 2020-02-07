using KandaEu.Volejbal.Web.WebApiClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Nastenka
{
	public class NastenkaState
	{
		public VzkazListDto Vzkazy { get; set; }
		public OsobaListDto AktivniOsoby { get; set; }
	}
}
