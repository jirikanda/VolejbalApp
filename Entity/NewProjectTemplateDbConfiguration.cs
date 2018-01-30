using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Havit.NewProjectTemplate.Entity
{
    public class NewProjectTemplateDbConfiguration : DbConfiguration
    {
        /// <summary>
        /// Tooling pro code migrations vyžaduje bezparametrický konstruktor.
        /// </summary>
        public NewProjectTemplateDbConfiguration() : this(string.Empty, null)
        {
        }

        public NewProjectTemplateDbConfiguration(string connectionString, string modelStoreDirectory)
        {
            SetContextFactory(typeof(NewProjectTemplateDbContext), () => new NewProjectTemplateDbContextFactory(connectionString).Create());

            if (!String.IsNullOrEmpty(modelStoreDirectory))
            {
                Directory.CreateDirectory(modelStoreDirectory);
                SetModelStore(new DefaultDbModelStore(modelStoreDirectory));
            }            
        }

    }
}
