using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Nastenka.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class VlozVzkazRequest
	{
		public int AutorId { get; set; }

		public string Zprava { get; set; }
	}
}
