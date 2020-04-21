using KandaEu.Volejbal.Contracts.Osoby.Dto;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto
{
	[DataContract]
	public class GetDetailTerminuResult
	{
		[DataMember(Order = 1)]
		public List<OsobaDto> Prihlaseni { get; set; } = new List<OsobaDto>();

		[DataMember(Order = 2)]
		public List<OsobaDto> Neprihlaseni { get; set; } = new List<OsobaDto>();
	}
}