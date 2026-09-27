namespace KandaEu.Volejbal.Model.Metadata;

/// <summary>
/// Limity délek textů vzkazu. Dřív generované z EF modelu; Cosmos žádné délky nevynucuje, ale dokument
/// má limit 2 MB, takže zpráva má rozumný strop místo dřívějšího nvarchar(max).
/// </summary>
public static class VzkazMetadata
{
	public const int ZpravaMaxLength = 4000;
}
