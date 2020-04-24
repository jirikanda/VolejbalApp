using ProtoBuf;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class GetTerminyResponse
	{
		public List<TerminDto> Terminy { get; set; } = new List<TerminDto>();
	}
}
