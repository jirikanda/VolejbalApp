using System;
using System.Collections.Generic;
using System.Text;

namespace Contracts.Nastenka.Dto
{
	public class VzkazDto
	{
		public int Id { get; set; }
		public string Author { get; set; }
		public string Zprava { get; set; }
		public DateTime PostedAt { get; set; }

	}
}
