using Blazor.Fluxor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{
	public class LoadTerminyReducer : Reducer<TerminyState, LoadTerminyAction>
	{
		public override TerminyState Reduce(TerminyState state, LoadTerminyAction action)
		{			
			if (state.Terminy != null)
			{
				return state;
			}

			return new TerminyState(true, false, null);
		}

		
	}
}
