using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Blazor.StateManagement.Prihlasovani
{
	public class PrihlasovaniState
	{
		public int? AktualniTerminId { get; set; }
		public bool IsLoading { get; set; }
		public bool LoadingFailed { get; set; }

		public List<OsobaDto> Prihlaseni { get; set; }
		public List<OsobaDto> Neprihlaseni { get; set; }

	}
}
