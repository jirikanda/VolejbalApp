using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto
{
	[DataContract]
	public class TerminDto
	{
		[DataMember(Order = 1)]
		public int Id { get; set; }
		
		[DataMember(Order = 2)]
		public DateTime Datum { get; set; }
	}
}
