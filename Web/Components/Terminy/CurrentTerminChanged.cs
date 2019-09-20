namespace KandaEu.Web.Components.Terminy
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
