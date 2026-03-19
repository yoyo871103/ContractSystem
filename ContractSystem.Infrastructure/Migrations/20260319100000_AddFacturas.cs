using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturas : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var decimalType = isSqlite ? "TEXT" : "decimal(18,2)";

            migrationBuilder.CreateTable(
                name: "Facturas",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Numero = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(100)", maxLength: 100, nullable: false),
                    Fecha = table.Column<DateTime>(type: isSqlite ? "TEXT" : "datetime2", nullable: false),
                    ImporteTotal = table.Column<decimal>(type: decimalType, precision: 18, scale: 2, nullable: false),
                    Descripcion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ContratoId = table.Column<int>(type: intType, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: isSqlite ? "TEXT" : "datetime2", nullable: false),
                    CreadoPor = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(max)", nullable: true),
                    FechaModificacion = table.Column<DateTime>(type: isSqlite ? "TEXT" : "datetime2", nullable: true),
                    ModificadoPor = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Facturas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Facturas_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Facturas_ContratoId",
                table: "Facturas",
                column: "ContratoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "Facturas");
        }
    }
}
