using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLicencias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";

            migrationBuilder.CreateTable(
                name: "Licencias",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Clave = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(1024)", maxLength: 1024, nullable: false),
                    FechaActivacion = table.Column<DateTime>(type: isSqlite ? "TEXT" : "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: isSqlite ? "TEXT" : "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Licencias", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Licencias");
        }
    }
}
