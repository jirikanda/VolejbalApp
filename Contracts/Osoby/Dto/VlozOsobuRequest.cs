using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Osoby.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class VlozOsobuRequest
	{
		public string Jmeno { get; set; }

		public string Prijmeni { get; set; }

		public string Email { get; set; }
	}
}
