using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Prihlasky.Dto
{
	[DataContract]
	public class PrihlasitOdhlasitRequest
	{
		[DataMember(Order = 1)]
		public int TerminId { get; set; }
		
		[DataMember(Order = 2)]
		public int OsobaId { get; set; }
	}
}
