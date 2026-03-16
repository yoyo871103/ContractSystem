using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNumeracionConfigurable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var dateOffsetType = isSqlite ? "TEXT" : "datetimeoffset";
            var bitType = isSqlite ? "INTEGER" : "bit";

            migrationBuilder.CreateTable(
                name: "ConfiguracionNumeracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Formato = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(512)", maxLength: 512, nullable: false),
                    DigitosPadding = table.Column<int>(type: intType, nullable: false),
                    ContadorPorAnio = table.Column<bool>(type: bitType, nullable: false),
                    Activa = table.Column<bool>(type: bitType, nullable: false),
                    FechaCreacion = table.Column<DateTimeOffset>(type: dateOffsetType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionNumeracion", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ContadoresNumeracion",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Anio = table.Column<int>(type: intType, nullable: true),
                    UltimoNumero = table.Column<int>(type: intType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContadoresNumeracion", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContadoresNumeracion_Anio",
                table: "ContadoresNumeracion",
                column: "Anio",
                unique: true,
                filter: isSqlite ? null : "[Anio] IS NOT NULL");

            // Insertar configuración por defecto (raw SQL para evitar dependencia de entity mapping)
            if (isSqlite)
            {
                migrationBuilder.Sql(
                    "INSERT INTO ConfiguracionNumeracion (Formato, DigitosPadding, ContadorPorAnio, Activa, FechaCreacion) " +
                    "VALUES ('CON-{TIPO}-{YYYY}-{CONTADOR}', 4, 1, 1, datetime('now'));");
            }
            else
            {
                migrationBuilder.Sql(
                    "INSERT INTO ConfiguracionNumeracion (Formato, DigitosPadding, ContadorPorAnio, Activa, FechaCreacion) " +
                    "VALUES (N'CON-{TIPO}-{YYYY}-{CONTADOR}', 4, 1, 1, SYSDATETIMEOFFSET());");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ContadoresNumeracion");
            migrationBuilder.DropTable(name: "ConfiguracionNumeracion");
        }
    }
}
