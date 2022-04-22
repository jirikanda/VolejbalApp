using Havit.Data.EntityFrameworkCore;

namespace KandaEu.Volejbal.Entity;

public class VolejbalDbContext : Havit.Data.EntityFrameworkCore.DbContext
{
    /// <summary>
    /// Konstruktor.
    /// Pro použití v unit testech, jiné použití nemá.
    /// </summary>
    internal VolejbalDbContext()
    {
        // NOOP
    }

    /// <summary>
    /// Konstruktor.
    /// </summary>
    public VolejbalDbContext(DbContextOptions options) : base(options)
    {
        // NOOP
    }

    /// <inheritdoc />
    protected override void CustomizeModelCreating(ModelBuilder modelBuilder)
    {
        base.CustomizeModelCreating(modelBuilder);

        modelBuilder.RegisterModelFromAssembly(typeof(KandaEu.Volejbal.Model.Termin).Assembly);
        modelBuilder.ApplyConfigurationsFromAssembly(this.GetType().Assembly);
    }
}
