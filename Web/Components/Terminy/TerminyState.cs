using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Terminy
{
	public class TerminyState
	{
		public List<TerminDto> Terminy { get; set; }
		public int? CurrentTerminId { get; set; }
	}
}
