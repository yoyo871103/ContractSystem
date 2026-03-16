using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNomPlantillasDocumento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var blobType = isSqlite ? "BLOB" : "varbinary(max)";
            var dateType = isSqlite ? "TEXT" : "datetime2";
            var bitType = isSqlite ? "INTEGER" : "bit";

            migrationBuilder.CreateTable(
                name: "nom_PlantillasDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Nombre = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    TipoDocumento = table.Column<int>(type: intType, nullable: false),
                    Rol = table.Column<int>(type: intType, nullable: false),
                    Archivo = table.Column<byte[]>(type: blobType, nullable: false),
                    NombreArchivo = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(512)", maxLength: 512, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: dateType, nullable: false),
                    RevisadoPorLegal = table.Column<bool>(type: bitType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_PlantillasDocumento", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_PlantillasDocumento_TipoDocumento",
                table: "nom_PlantillasDocumento",
                column: "TipoDocumento");

            migrationBuilder.CreateIndex(
                name: "IX_nom_PlantillasDocumento_Rol",
                table: "nom_PlantillasDocumento",
                column: "Rol");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "nom_PlantillasDocumento");
        }
    }
}
