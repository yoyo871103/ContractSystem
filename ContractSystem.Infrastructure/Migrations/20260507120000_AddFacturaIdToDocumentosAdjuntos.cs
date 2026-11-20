using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFacturaIdToDocumentosAdjuntos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";

            migrationBuilder.AddColumn<int>(
                name: "FacturaId",
                table: "DocumentosAdjuntos",
                type: intType,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAdjuntos_FacturaId",
                table: "DocumentosAdjuntos",
                column: "FacturaId");

            // NO ACTION para evitar múltiples rutas de cascade en SQL Server
            // (Contratos -> DocumentosAdjuntos ya cascadea, y Contratos -> Facturas también).
            // El borrado de adjuntos al eliminar una factura se gestiona en código.
            // SQLite no admite ALTER TABLE ... ADD CONSTRAINT: allí la columna queda sin FK
            // (la integridad la garantiza el código, que borra los adjuntos al eliminar la factura).
            if (!isSqlite)
            {
                migrationBuilder.AddForeignKey(
                    name: "FK_DocumentosAdjuntos_Facturas_FacturaId",
                    table: "DocumentosAdjuntos",
                    column: "FacturaId",
                    principalTable: "Facturas",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            if (!isSqlite)
            {
                migrationBuilder.DropForeignKey(
                    name: "FK_DocumentosAdjuntos_Facturas_FacturaId",
                    table: "DocumentosAdjuntos");
            }

            migrationBuilder.DropIndex(
                name: "IX_DocumentosAdjuntos_FacturaId",
                table: "DocumentosAdjuntos");

            migrationBuilder.DropColumn(
                name: "FacturaId",
                table: "DocumentosAdjuntos");
        }
    }
}
