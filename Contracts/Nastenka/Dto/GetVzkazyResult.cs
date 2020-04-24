using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Nastenka.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class GetVzkazyResult
	{		
		public List<VzkazDto> Vzkazy { get; set; } = new List<VzkazDto>();
	}
}
