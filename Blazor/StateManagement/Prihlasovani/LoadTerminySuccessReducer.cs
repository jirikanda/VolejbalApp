using Blazor.Fluxor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{
	public class LoadTerminySuccessReducer : Reducer<TerminyState, LoadTerminySuccessAction>
	{
		public override TerminyState Reduce(TerminyState state, LoadTerminySuccessAction action)
		{
			return new TerminyState(false, false, action.Terminy);
		}
	}
}
