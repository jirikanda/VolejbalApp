using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Reporty.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ReportTerminuResponse
	{
		public List<ReportTerminuItem> ObsazenostTerminu { get; set; } = new List<ReportTerminuItem>();
	}
}
