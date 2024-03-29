﻿namespace KandaEu.Volejbal.Model;

public class Vzkaz
{
	public virtual int Id { get; set; }

	public Osoba Autor { get; set; }
	public int AutorId { get; set; }

	public DateTime DatumVlozeni { get; set; }

	[Required]
	[MaxLength]
	public string Zprava { get; set; }

	public DateTime? Deleted { get; set; }
}
