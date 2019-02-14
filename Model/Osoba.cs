using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KandaEu.Volejbal.Model
{
	public class Osoba
    {		
		public int Id { get; set; }

		[Required]
		[MaxLength(50)]
		public string Prijmeni { get; set; }

		[Required]
		[MaxLength(50)]
		public string Jmeno { get; set; }

		[Required]
		[MaxLength(50)]
		public string Email { get; set; }

		public DateTime? Deleted { get; set; }

		public string PrijmeniJmeno
		{
			get
			{
				return (this.Prijmeni + " " + this.Jmeno).Trim();
			}
		}

    }
}
