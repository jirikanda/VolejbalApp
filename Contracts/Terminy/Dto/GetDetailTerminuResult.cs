using KandaEu.Volejbal.Contracts.Osoby.Dto;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class GetDetailTerminuResult
	{
		public List<OsobaDto> Prihlaseni { get; set; } = new List<OsobaDto>();

		public List<OsobaDto> Neprihlaseni { get; set; } = new List<OsobaDto>();
	}
}