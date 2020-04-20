using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Osoby.Dto
{
	[DataContract]
	public class OsobaDto
	{
		[DataMember(Order = 1)]
		public int Id { get; set; }
		
		[DataMember(Order = 2)]
		public string PrijmeniJmeno { get; set; }
	}
}