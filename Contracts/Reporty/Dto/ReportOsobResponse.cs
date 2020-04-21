using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Reporty.Dto
{
	[DataContract]
	public class ReportOsobResponse
	{
		[DataMember(Order = 1)]
		public List<ReportOsobItem> UcastHracu { get; set; } = new List<ReportOsobItem>();
	}
}
