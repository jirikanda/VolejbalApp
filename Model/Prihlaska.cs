namespace KandaEu.Volejbal.Model;

public class Prihlaska
{
    public int Id { get; set; }

    public Osoba Osoba { get; set; }
    public int OsobaId { get; set; }

    public Termin Termin { get; set; }
    public int TerminId { get; set; }

    public DateTime DatumPrihlaseni { get; set; }

    public DateTime? Deleted { get; set; }
}
