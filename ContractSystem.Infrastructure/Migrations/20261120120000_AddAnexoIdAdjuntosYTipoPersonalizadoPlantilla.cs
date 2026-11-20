using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <summary>
    /// - DocumentosAdjuntos.AnexoId: permite asociar adjuntos a un anexo concreto.
    /// - nom_PlantillasDocumento.TipoPersonalizado: nombre del tipo cuando TipoDocumento = Otros (4).
    /// </summary>
    public partial class AddAnexoIdAdjuntosYTipoPersonalizadoPlantilla : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var textType = isSqlite ? "TEXT" : "nvarchar(150)";

            migrationBuilder.AddColumn<int>(
                name: "AnexoId",
                table: "DocumentosAdjuntos",
                type: intType,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAdjuntos_AnexoId",
                table: "DocumentosAdjuntos",
                column: "AnexoId");

            // NO ACTION para evitar múltiples rutas de cascade en SQL Server
            // (Contratos -> Anexos y Contratos -> DocumentosAdjuntos ya cascadean).
            // El borrado de adjuntos al eliminar un anexo se gestiona en código (AnexoStore).
            // SQLite no admite ALTER TABLE ... ADD CONSTRAINT: allí la columna queda sin FK
            // (la integridad la garantiza el código, que borra los adjuntos al eliminar el anexo).
            if (!isSqlite)
            {
                migrationBuilder.AddForeignKey(
                    name: "FK_DocumentosAdjuntos_Anexos_AnexoId",
                    table: "DocumentosAdjuntos",
                    column: "AnexoId",
                    principalTable: "Anexos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            }

            migrationBuilder.AddColumn<string>(
                name: "TipoPersonalizado",
                table: "nom_PlantillasDocumento",
                type: textType,
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            if (!isSqlite)
            {
                migrationBuilder.DropForeignKey(
                    name: "FK_DocumentosAdjuntos_Anexos_AnexoId",
                    table: "DocumentosAdjuntos");
            }

            migrationBuilder.DropIndex(
                name: "IX_DocumentosAdjuntos_AnexoId",
                table: "DocumentosAdjuntos");

            migrationBuilder.DropColumn(
                name: "AnexoId",
                table: "DocumentosAdjuntos");

            migrationBuilder.DropColumn(
                name: "TipoPersonalizado",
                table: "nom_PlantillasDocumento");
        }
    }
}
