using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Reporty.Dto
{
	[DataContract]
	public class ReportTerminuItem
	{
		[DataMember(Order = 1)]
		public DateTime Datum { get; set; }
		
		[DataMember(Order = 2)]
		public int PocetHracu { get; set; }
	}
}
