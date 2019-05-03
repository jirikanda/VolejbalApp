using System.Collections.Generic;

namespace KandaEu.Volejbal.Facades.Terminy.Dto
{
	public class TerminDetailDto
	{
		public List<OsobaDto> Prihlaseni { get; set; }
		public List<OsobaDto> Neprihlaseni { get; set; }
	}
}