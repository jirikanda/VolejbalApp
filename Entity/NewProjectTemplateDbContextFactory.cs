using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havit.NewProjectTemplate.Entity
{
    public class NewProjectTemplateDbContextFactory : IDbContextFactory<NewProjectTemplateDbContext>
    {
        private readonly string connectionString;

        public NewProjectTemplateDbContextFactory(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public NewProjectTemplateDbContext Create()
        {
            return String.IsNullOrEmpty(connectionString) ? new NewProjectTemplateDbContext() : new NewProjectTemplateDbContext(connectionString);
        }
    }
}
