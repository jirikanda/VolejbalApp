﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Havit.Data.EntityFrameworkCore;
using Havit.Data.EntityFrameworkCore.Patterns.Caching;
using Havit.Data.EntityFrameworkCore.Patterns.Repositories;
using Havit.Data.EntityFrameworkCore.Patterns.SoftDeletes;
using Havit.Data.Patterns.DataEntries;
using Havit.Data.Patterns.DataLoaders;
using Havit.Data.Patterns.Infrastructure;

namespace KandaEu.Volejbal.DataLayer.Repositories
{
	[System.CodeDom.Compiler.GeneratedCode("Havit.Data.EntityFrameworkCore.CodeGenerator", "1.0")]
	public partial class TerminDbRepository : TerminDbRepositoryBase, ITerminRepository
	{
		public TerminDbRepository(IDbContext dbContext, KandaEu.Volejbal.DataLayer.DataSources.ITerminDataSource dataSource, IEntityKeyAccessor<KandaEu.Volejbal.Model.Termin, int> entityKeyAccessor, IDataLoader dataLoader, ISoftDeleteManager softDeleteManager, IEntityCacheManager entityCacheManager)
			: base(dbContext, dataSource, entityKeyAccessor, dataLoader, softDeleteManager, entityCacheManager)
		{
		}
	}
}