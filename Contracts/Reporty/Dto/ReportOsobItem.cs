using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Reporty.Dto
{
	[DataContract]
	public class ReportOsobItem
	{
		[DataMember(Order = 1)]
		public string PrijmeniJmeno { get; set; }
		
		[DataMember(Order = 2)]
		public int PocetTerminu { get; set; }
	}
}
