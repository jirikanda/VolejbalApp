using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Reporty.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class ReportOsobResponse
	{
		public List<ReportOsobItem> UcastHracu { get; set; } = new List<ReportOsobItem>();
	}
}
