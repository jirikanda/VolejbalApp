namespace KandaEu.Volejbal.DataLayer;

/// <summary>
/// Objekt (osoba, termín) s daným id neexistuje. Vyhazují repozitáře při načtení podle id, které
/// přichází z URL, tedy zvenčí - fasády tak s null nepracují a "nenalezeno" se řeší na jednom místě
/// (ExceptionHandlingMiddleware ji mapuje na 422 stejně jako OperationFailedException).
/// </summary>
/// <remarks>
/// Vlastní typ místo Havit.Data.Patterns.Exceptions.ObjectNotFoundException: kvůli jedné výjimce
/// se balíček z EF éry zpět netahá.
/// </remarks>
public class ObjectNotFoundException : Exception
{
	public ObjectNotFoundException(string message) : base(message)
	{
	}

	public ObjectNotFoundException(string message, Exception innerException) : base(message, innerException)
	{
	}
}
