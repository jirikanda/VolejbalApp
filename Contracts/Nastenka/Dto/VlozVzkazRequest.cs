using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Nastenka.Dto
{
	[DataContract]
	public class VlozVzkazRequest
	{
		[DataMember(Order = 1)]
		public int AutorId { get; set; }

		[DataMember(Order = 2)]
		public string Zprava { get; set; }
	}
}
