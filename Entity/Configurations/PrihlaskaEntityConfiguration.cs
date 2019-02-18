using KandaEu.Volejbal.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Text;

namespace KandaEu.Volejbal.Entity.Configurations
{
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
			.HasName("UIDX_Prihlaska_TerminId_OsobaId_Deleted")
			.IsUnique()
			.HasFilter(null);
		}
	}
}
