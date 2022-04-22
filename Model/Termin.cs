namespace KandaEu.Volejbal.Model;

public class Termin
{
    public int Id { get; set; }

    public DateTime Datum { get; set; }

    public DateTime? Deleted { get; set; }

    public List<Prihlaska> Prihlasky { get; } = new List<Prihlaska>();
}
