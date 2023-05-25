namespace KandaEu.Volejbal.Services.Infrastructure;

public class OperationFailedException : ApplicationException
{
	public OperationFailedException(string message) : base(message)
	{
		// NOOP
	}

	public OperationFailedException(string message, Exception innerException) : base(message, innerException)
	{
		// NOOP
	}
}
