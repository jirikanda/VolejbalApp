using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Reporty.Dto
{
	// TODO: Nested class?

	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ReportTerminuItem
	{
		public DateTime Datum { get; set; }
		
		public int PocetHracu { get; set; }
	}
}
