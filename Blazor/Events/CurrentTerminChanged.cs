namespace Blazor.Events
{
	public class CurrentTerminChanged
	{
		public CurrentTerminChanged(int terminId)
		{
			TerminId = terminId;
		}

		public int TerminId { get; }
	}
}
