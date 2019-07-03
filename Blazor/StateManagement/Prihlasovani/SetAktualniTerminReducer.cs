using Blazor.Fluxor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{
	public class SetAktualniTerminReducer : Reducer<AktualniTerminState, SetAktualniTerminAction>
	{
		public override AktualniTerminState Reduce(AktualniTerminState state, SetAktualniTerminAction action)
		{
			return new AktualniTerminState(action.AktualniTerminId);
		}
	}
}
