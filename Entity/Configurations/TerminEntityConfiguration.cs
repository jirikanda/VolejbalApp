using KandaEu.Volejbal.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Entity.Configurations
{
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
}
