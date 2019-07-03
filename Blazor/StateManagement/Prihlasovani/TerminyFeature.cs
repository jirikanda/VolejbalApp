using Blazor.Fluxor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{
	public class TerminyFeature : Feature<TerminyState>
	{
		public override string GetName()
		{
			return this.GetType().FullName;
		}

		protected override TerminyState GetInitialState()
		{
			return new TerminyState(false, false, null);
		}
	}
}
