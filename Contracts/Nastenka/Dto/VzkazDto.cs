using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Nastenka.Dto
{
	[DataContract]
	public class VzkazDto
	{
		[DataMember(Order = 1)]
		public string Author { get; set; }
		
		[DataMember(Order = 2)]
		public string Zprava { get; set; }
		
		[DataMember(Order = 3)]
		public DateTime DatumVlozeni { get; set; }
	}
}
