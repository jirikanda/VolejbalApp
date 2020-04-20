using KandaEu.Volejbal.Contracts.Osoby.Dto;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto
{
	[DataContract]
	public class TerminDetailDto
	{
		[DataMember(Order = 1)]
		public List<OsobaDto> Prihlaseni { get; set; }

		[DataMember(Order = 2)]
		public List<OsobaDto> Neprihlaseni { get; set; }
	}
}