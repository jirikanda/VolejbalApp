using KandaEu.Volejbal.Contracts.Terminy.Dto;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Osoby.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class GetOsobyResponse
	{
		public List<OsobaDto> Osoby { get; set; } = new List<OsobaDto>();
	}
}
