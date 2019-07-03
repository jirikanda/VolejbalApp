using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.StateManagement.Prihlasovani
{
	public class AktualniTerminState
	{
		public int? AktualniTerminId { get; private set; }

		public AktualniTerminState(int? aktualniTerminId)
		{
			AktualniTerminId = aktualniTerminId;
		}

	}
}
