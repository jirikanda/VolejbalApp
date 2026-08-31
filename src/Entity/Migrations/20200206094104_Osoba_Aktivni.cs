using Microsoft.EntityFrameworkCore.Migrations;

namespace KandaEu.Volejbal.Entity.Migrations;

public partial class Osoba_Aktivni : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<bool>(
            name: "Aktivni",
            table: "Osoba",
            nullable: false,
            defaultValue: false);
        migrationBuilder.Sql("UPDATE Osoba SET Aktivni = 1 WHERE Deleted IS NULL");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropColumn(
            name: "Aktivni",
            table: "Osoba");
    }
}
