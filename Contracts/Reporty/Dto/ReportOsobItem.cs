using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Reporty.Dto
{
	// TODO: Nested class?

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ReportOsobItem
	{
		public string PrijmeniJmeno { get; set; }
		
		public int PocetTerminu { get; set; }
	}
}
