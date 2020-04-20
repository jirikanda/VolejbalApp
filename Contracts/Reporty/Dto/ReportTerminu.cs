using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Reporty.Dto
{
	[DataContract]
	public class ReportTerminu
	{
		[DataMember(Order = 1)]
		public List<ReportTerminuItem> ObsazenostTerminu { get; set; }
	}
}
