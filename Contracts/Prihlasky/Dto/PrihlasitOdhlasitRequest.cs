using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Contracts.Prihlasky.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class PrihlasitOdhlasitRequest
	{
		public int TerminId { get; set; }
		
		public int OsobaId { get; set; }
	}
}
