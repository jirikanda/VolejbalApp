using Blazor.Fluxor;
using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{
	public class LoadTerminySuccessAction : IAction
	{
		public List<TerminDto> Terminy { get; }
	
		public LoadTerminySuccessAction(List<TerminDto> terminy)
		{
			Terminy = terminy;
		}

	}
}
