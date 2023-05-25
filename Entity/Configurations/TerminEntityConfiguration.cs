namespace KandaEu.Volejbal.Entity.Configurations;

public class TerminEntityConfiguration : IEntityTypeConfiguration<Termin>
{
	public void Configure(EntityTypeBuilder<Termin> builder)
	{
		builder.HasIndex(termin => new { termin.Datum, termin.Deleted })
			.HasDatabaseName("UIDX_Termin_Datum_Deleted")
			.IsUnique()
			.HasFilter(null);
	}
}
