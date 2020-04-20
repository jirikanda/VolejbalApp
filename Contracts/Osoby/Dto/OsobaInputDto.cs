using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Osoby.Dto
{
	[DataContract]
	public class OsobaInputDto
	{
		[DataMember(Order = 1)]
		public string Jmeno { get; set; }

		[DataMember(Order = 2)]
		public string Prijmeni { get; set; }

		[DataMember(Order = 3)]
		public string Email { get; set; }
	}
}
