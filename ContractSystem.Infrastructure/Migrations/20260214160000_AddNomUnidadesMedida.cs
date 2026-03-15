using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNomUnidadesMedida : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var textType = isSqlite ? "TEXT" : "nvarchar";
            var dateType = isSqlite ? "TEXT" : "datetimeoffset";

            migrationBuilder.CreateTable(
                name: "nom_UnidadesMedida",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    NombreCorto = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(32)", maxLength: 32, nullable: false),
                    Descripcion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    DeletedAt = table.Column<DateTimeOffset?>(type: dateType, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_UnidadesMedida", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_UnidadesMedida_NombreCorto",
                table: "nom_UnidadesMedida",
                column: "NombreCorto",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "nom_UnidadesMedida");
        }
    }
}
