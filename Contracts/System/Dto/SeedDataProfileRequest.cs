using System.Runtime.Serialization;

namespace KandaEu.Volejbal.Contracts.System.Dto
{
	[DataContract]
    public class SeedDataProfileRequest
    {
        [DataMember(Order = 1)]
        public string ProfileName { get; set; }
    }
}