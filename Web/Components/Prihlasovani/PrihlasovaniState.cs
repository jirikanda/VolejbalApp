﻿using KandaEu.Volejbal.Web.WebApiClients;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Prihlasovani
{
	public class PrihlasovaniState
	{
		public int? AktualniTerminId { get; set; }

		public List<OsobaDto> Prihlaseni { get; set; }
		public List<OsobaDto> Neprihlaseni { get; set; }

	}
}
