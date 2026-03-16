using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNomTerceros : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var dateType = isSqlite ? "TEXT" : "datetimeoffset";

            migrationBuilder.CreateTable(
                name: "nom_Terceros",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Nombre = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    RazonSocial = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    NifCif = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(32)", maxLength: 32, nullable: false),
                    Direccion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(512)", maxLength: 512, nullable: false),
                    Telefono = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    Tipo = table.Column<int>(type: intType, nullable: false),
                    DeletedAt = table.Column<DateTimeOffset?>(type: dateType, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_Terceros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nom_ContactosTercero",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    TerceroId = table.Column<int>(type: intType, nullable: false),
                    Nombre = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    Cargo = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(128)", maxLength: 128, nullable: false),
                    Email = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    Telefono = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_ContactosTercero", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nom_ContactosTercero_nom_Terceros_TerceroId",
                        column: x => x.TerceroId,
                        principalTable: "nom_Terceros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_Terceros_NifCif",
                table: "nom_Terceros",
                column: "NifCif");

            migrationBuilder.CreateIndex(
                name: "IX_nom_Terceros_Tipo",
                table: "nom_Terceros",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_nom_ContactosTercero_TerceroId",
                table: "nom_ContactosTercero",
                column: "TerceroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "nom_ContactosTercero");
            migrationBuilder.DropTable(name: "nom_Terceros");
        }
    }
}
