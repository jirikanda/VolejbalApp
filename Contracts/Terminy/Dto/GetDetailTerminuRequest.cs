using ProtoBuf;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class GetDetailTerminuRequest
	{
		public int TerminId { get; set; }
	}
}