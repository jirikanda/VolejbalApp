using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{
	public class TerminyState
	{
		public bool IsLoading { get; private set; }
		public bool LoadingFailed { get; private set; }
		public List<TerminDto> Terminy { get; set; }

		public TerminyState(bool isLoading, bool loadingFailed, List<TerminDto> terminy)
		{
			IsLoading = isLoading;
			LoadingFailed = loadingFailed;
			Terminy = terminy;
		}
	}
}
