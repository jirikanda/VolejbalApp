using System.Runtime.Serialization;

namespace KandaEu.Volejbal.Contracts.Terminy.Dto
{
	[DataContract]
	public class GetDetailTerminuRequest
	{
		[DataMember(Order = 1)]
		public int TerminId { get; set; }
	}
}