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

namespace KandaEu.Volejbal.DataLayer.Repositories
{
	public partial class PrihlaskaDbRepository : IPrihlaskaRepository
	{
		public Prihlaska GetPrihlaska(int terminId, int osobaId)
		{
			return Data.Where(item => (item.TerminId == terminId) && (item.OsobaId == osobaId)).SingleOrDefault();
		}
	}
}