using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHistorialCambios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";

            migrationBuilder.CreateTable(
                name: "HistorialCambios",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    FechaHora = table.Column<DateTime>(type: isSqlite ? "TEXT" : "datetime2", nullable: false),
                    UsuarioId = table.Column<int>(type: intType, nullable: true),
                    UsuarioNombre = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(200)", maxLength: 200, nullable: true),
                    TipoCambio = table.Column<int>(type: intType, nullable: false),
                    ContratoId = table.Column<int>(type: intType, nullable: false),
                    Descripcion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ValorAnterior = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(max)", nullable: true),
                    ValorNuevo = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialCambios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialCambios_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCambios_ContratoId",
                table: "HistorialCambios",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCambios_FechaHora",
                table: "HistorialCambios",
                column: "FechaHora");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "HistorialCambios");
        }
    }
}
