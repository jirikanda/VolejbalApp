using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Nastenka.Dto
{
	// TODO: Nested class?

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class VzkazDto
	{
		public string Author { get; set; }
		
		public string Zprava { get; set; }
		
		public DateTime DatumVlozeni { get; set; }
	}
}
