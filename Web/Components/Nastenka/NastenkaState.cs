using Contracts.Nastenka.Dto;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KandaEu.Volejbal.Web.Components.Nastenka
{
	public class NastenkaState
	{
		public bool IsLoading { get; set; }
		public bool LoadingFailed { get; set; }
		public List<VzkazDto> Vzkazy { get; set; }
	}
}
