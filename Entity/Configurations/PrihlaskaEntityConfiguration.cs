namespace KandaEu.Volejbal.Entity.Configurations;

public class PrihlaskaEntityConfiguration : IEntityTypeConfiguration<Prihlaska>
{
    public void Configure(EntityTypeBuilder<Prihlaska> builder)
    {
        builder.HasIndex(prihlaska => new
        {
            prihlaska.TerminId,
            prihlaska.OsobaId,
            prihlaska.Deleted
        })
        .HasDatabaseName("UIDX_Prihlaska_TerminId_OsobaId_Deleted")
        .IsUnique()
        .HasFilter(null);
    }
}
