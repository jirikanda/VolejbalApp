using System.Runtime.Serialization;

namespace KandaEu.Volejbal.Contracts.Osoby.Dto
{
	[DataContract]
	public class AktivujNeaktivniOsobuRequest
	{
		[DataMember(Order = 1)]
		public int OsobaId { get; set; }
	}
}