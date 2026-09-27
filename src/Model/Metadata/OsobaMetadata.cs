namespace KandaEu.Volejbal.Model.Metadata;

/// <summary>
/// Limity délek textů osoby. Dřív generované z EF modelu; Cosmos žádné délky nevynucuje, takže je
/// hlídá jen validace vstupních DTO v Contracts (a při exportu do SQL Serveru sloupce nvarchar(50)).
/// </summary>
public static class OsobaMetadata
{
	public const int EmailMaxLength = 50;
	public const int JmenoMaxLength = 50;
	public const int PrijmeniMaxLength = 50;
}
