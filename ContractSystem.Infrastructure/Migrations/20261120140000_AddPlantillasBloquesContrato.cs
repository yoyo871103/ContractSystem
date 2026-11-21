using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <summary>
    /// BusinessInfos: plantillas de los bloques de comparecencia y firma (null = texto predeterminado).
    /// </summary>
    public partial class AddPlantillasBloquesContrato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

            migrationBuilder.AddColumn<string>(
                name: "PlantillaComparecencia",
                table: "BusinessInfos",
                type: isSqlite ? "TEXT" : "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PlantillaFirma",
                table: "BusinessInfos",
                type: isSqlite ? "TEXT" : "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "PlantillaComparecencia", table: "BusinessInfos");
            migrationBuilder.DropColumn(name: "PlantillaFirma", table: "BusinessInfos");
        }
    }
}
