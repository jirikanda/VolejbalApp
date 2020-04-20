using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Havit.Data.EntityFrameworkCore;
using Havit.Data.EntityFrameworkCore.Patterns.Repositories;
using Havit.Data.EntityFrameworkCore.Patterns.SoftDeletes;
using Havit.Data.Patterns.DataEntries;
using Havit.Data.Patterns.DataLoaders;
using KandaEu.Volejbal.Model;
using Microsoft.EntityFrameworkCore;

namespace KandaEu.Volejbal.DataLayer.Repositories
{
	public partial class PrihlaskaDbRepository : IPrihlaskaRepository
	{
		public async Task<Prihlaska> GetPrihlaska(int terminId, int osobaId)
		{
			return await Data.Where(item => (item.TerminId == terminId) && (item.OsobaId == osobaId)).SingleOrDefaultAsync();
		}
	}
}