using ProtoBuf;

namespace KandaEu.Volejbal.Contracts.System.Dto
{
	[ProtoContract(ImplicitFields = ImplicitFields.AllPublic)]
    public class SeedDataProfileRequest
    {
        public string ProfileName { get; set; }
    }
}