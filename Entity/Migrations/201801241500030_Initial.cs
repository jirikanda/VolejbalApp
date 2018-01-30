namespace Havit.NewProjectTemplate.Entity.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.__DataSeed",
                c => new
                    {
                        DataSeedVersionProfileName = c.String(nullable: false, maxLength: 250),
                        Version = c.String(),
                    })
                .PrimaryKey(t => t.DataSeedVersionProfileName);
            
            CreateTable(
                "dbo.LoginAccount",
                c => new
                    {
                        LoginAccountId = c.Int(nullable: false, identity: true),
                        Username = c.String(maxLength: 32),
                    })
                .PrimaryKey(t => t.LoginAccountId);
            
            CreateTable(
                "dbo.Language",
                c => new
                    {
                        LanguageId = c.Int(nullable: false, identity: true),
                        Name = c.String(maxLength: 200),
                        Culture = c.String(maxLength: 10),
                        UiCulture = c.String(maxLength: 10),
                        Symbol = c.String(maxLength: 50),
                    })
                .PrimaryKey(t => t.LanguageId)
                .Index(t => t.Symbol, unique: true, name: "UIDX_Language_Symbol");
            
            CreateTable(
                "dbo.ApplicationSettings",
                c => new
                    {
                        ApplicationSettingsId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.ApplicationSettingsId);
            
        }
        
        public override void Down()
        {
            DropIndex("dbo.Language", "UIDX_Language_Symbol");
            DropTable("dbo.ApplicationSettings");
            DropTable("dbo.Language");
            DropTable("dbo.LoginAccount");
            DropTable("dbo.__DataSeed");
        }
    }
}
