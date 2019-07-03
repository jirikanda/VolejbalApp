using Blazor.Fluxor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{	
	public class AktualniTerminFeature : Feature<AktualniTerminState>
	{
		public override string GetName()
		{
			return this.GetType().FullName;
		}

		protected override AktualniTerminState GetInitialState()
		{
			return new AktualniTerminState(null);
		}
	}
}
