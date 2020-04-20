using KandaEu.Volejbal.Contracts.Terminy.Dto;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Osoby.Dto
{
	[DataContract]
	public class OsobaListDto
	{
		[DataMember(Order = 1)]
		public List<OsobaDto> Osoby { get; set; }
	}
}
