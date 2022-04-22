using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Havit.Data.Patterns.Repositories;
using KandaEu.Volejbal.Model;

namespace KandaEu.Volejbal.DataLayer.Repositories;

public partial interface IPrihlaskaRepository
{
    Task<Prihlaska> GetPrihlaska(int terminId, int osobaId);
}
