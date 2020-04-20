using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Nastenka.Dto
{
	[DataContract]
	public class VzkazListDto
	{
		[DataMember(Order = 1)]
		public List<VzkazDto> Vzkazy { get; set; }
	}
}
