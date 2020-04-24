using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Osoby.Dto
{
	// TODO: Nested class?

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class OsobaDto
	{
		public int Id { get; set; }
		
		public string PrijmeniJmeno { get; set; }
	}
}