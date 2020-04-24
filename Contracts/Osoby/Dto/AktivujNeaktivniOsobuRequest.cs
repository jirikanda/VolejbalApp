using ProtoBuf;

namespace KandaEu.Volejbal.Contracts.Osoby.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
	public class AktivujNeaktivniOsobuRequest
	{
		public int OsobaId { get; set; }
	}
}