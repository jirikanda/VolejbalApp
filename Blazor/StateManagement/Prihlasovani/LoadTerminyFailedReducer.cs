using Blazor.Fluxor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{
	public class LoadTerminyFailedReducer : Reducer<TerminyState, LoadTerminyFailedAction>
	{
		public override TerminyState Reduce(TerminyState state, LoadTerminyFailedAction action)
		{
			return new TerminyState(false, true, null);
		}
	}
}
