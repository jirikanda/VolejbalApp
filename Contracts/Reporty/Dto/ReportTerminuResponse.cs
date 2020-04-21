using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Reporty.Dto
{
	[DataContract]
	public class ReportTerminuResponse
	{
		[DataMember(Order = 1)]
		public List<ReportTerminuItem> ObsazenostTerminu { get; set; } = new List<ReportTerminuItem>();
	}
}
