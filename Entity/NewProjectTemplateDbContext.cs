using System;
using System.Data.Entity;
using Havit.Data.Entity;
using Havit.NewProjectTemplate.Entity.Migrations;

namespace Havit.NewProjectTemplate.Entity
{
	public class NewProjectTemplateDbContext : Havit.Data.Entity.DbContext
	{
        /// <summary>
        /// Nastaví (na úrovni aplikace) použití Code Migrations strategie.
        /// </summary>
		public static void SetEntityFrameworkMigrations()
		{
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<NewProjectTemplateDbContext, Configuration>());
		}

	    /// <summary>
	    /// Spouští se z generátoru kódu před vytvořením instance databázového kontextu.
	    /// </summary>
	    public static void ConfigureForCodeGenerator(string connectionString)
	    {
	        DbConfiguration.SetConfiguration(new NewProjectTemplateDbConfiguration(connectionString, String.Empty));
	    }

        /// <summary>
        /// Konstruktor pro unit testy, nemá jiné využití.
        /// </summary>
	    internal NewProjectTemplateDbContext()
	    {
	        // NOOP
	    }

        /// <summary>
        /// Konstruktor.
        /// </summary>
        public NewProjectTemplateDbContext(string connectionString) : base(connectionString)
	    {
	        // NOOP
	    }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);
			
			modelBuilder.Configurations.AddFromAssembly(this.GetType().Assembly);
			modelBuilder.RegisterModelFromAssembly(typeof(Havit.NewProjectTemplate.Model.Localizations.Language).Assembly);
		}
	}
}