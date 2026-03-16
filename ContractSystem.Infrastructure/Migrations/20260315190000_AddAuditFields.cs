using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAuditFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite";

            // --- Contratos: ya tiene FechaCreacion, agregar CreadoPor, FechaModificacion, ModificadoPor ---
            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "Contratos",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "Contratos",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPorUsuario",
                table: "Contratos",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            // --- Terceros ---
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "nom_Terceros",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: false,
                defaultValueSql: isSqlite ? "datetime('now')" : "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "nom_Terceros",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "nom_Terceros",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "nom_Terceros",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            // --- ProductosServicios ---
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "nom_ProductosServicios",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: false,
                defaultValueSql: isSqlite ? "datetime('now')" : "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "nom_ProductosServicios",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "nom_ProductosServicios",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "nom_ProductosServicios",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            // --- Anexos ---
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Anexos",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: false,
                defaultValueSql: isSqlite ? "datetime('now')" : "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "Anexos",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "Anexos",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "Anexos",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            // --- LineasDetalle ---
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "LineasDetalle",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: false,
                defaultValueSql: isSqlite ? "datetime('now')" : "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "LineasDetalle",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "LineasDetalle",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "LineasDetalle",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            // --- DocumentosAdjuntos ---
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "DocumentosAdjuntos",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: false,
                defaultValueSql: isSqlite ? "datetime('now')" : "GETUTCDATE()");

            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "DocumentosAdjuntos",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "DocumentosAdjuntos",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "DocumentosAdjuntos",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            // --- PlantillasDocumento: ya tiene FechaCreacion, agregar CreadoPor, FechaModificacion, ModificadoPor ---
            migrationBuilder.AddColumn<string>(
                name: "CreadoPor",
                table: "nom_PlantillasDocumento",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaModificacion",
                table: "nom_PlantillasDocumento",
                type: isSqlite ? "TEXT" : "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModificadoPor",
                table: "nom_PlantillasDocumento",
                type: isSqlite ? "TEXT" : "nvarchar(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Contratos
            migrationBuilder.DropColumn(name: "CreadoPor", table: "Contratos");
            migrationBuilder.DropColumn(name: "FechaModificacion", table: "Contratos");
            migrationBuilder.DropColumn(name: "ModificadoPorUsuario", table: "Contratos");

            // Terceros
            migrationBuilder.DropColumn(name: "FechaCreacion", table: "nom_Terceros");
            migrationBuilder.DropColumn(name: "CreadoPor", table: "nom_Terceros");
            migrationBuilder.DropColumn(name: "FechaModificacion", table: "nom_Terceros");
            migrationBuilder.DropColumn(name: "ModificadoPor", table: "nom_Terceros");

            // ProductosServicios
            migrationBuilder.DropColumn(name: "FechaCreacion", table: "nom_ProductosServicios");
            migrationBuilder.DropColumn(name: "CreadoPor", table: "nom_ProductosServicios");
            migrationBuilder.DropColumn(name: "FechaModificacion", table: "nom_ProductosServicios");
            migrationBuilder.DropColumn(name: "ModificadoPor", table: "nom_ProductosServicios");

            // Anexos
            migrationBuilder.DropColumn(name: "FechaCreacion", table: "Anexos");
            migrationBuilder.DropColumn(name: "CreadoPor", table: "Anexos");
            migrationBuilder.DropColumn(name: "FechaModificacion", table: "Anexos");
            migrationBuilder.DropColumn(name: "ModificadoPor", table: "Anexos");

            // LineasDetalle
            migrationBuilder.DropColumn(name: "FechaCreacion", table: "LineasDetalle");
            migrationBuilder.DropColumn(name: "CreadoPor", table: "LineasDetalle");
            migrationBuilder.DropColumn(name: "FechaModificacion", table: "LineasDetalle");
            migrationBuilder.DropColumn(name: "ModificadoPor", table: "LineasDetalle");

            // DocumentosAdjuntos
            migrationBuilder.DropColumn(name: "FechaCreacion", table: "DocumentosAdjuntos");
            migrationBuilder.DropColumn(name: "CreadoPor", table: "DocumentosAdjuntos");
            migrationBuilder.DropColumn(name: "FechaModificacion", table: "DocumentosAdjuntos");
            migrationBuilder.DropColumn(name: "ModificadoPor", table: "DocumentosAdjuntos");

            // PlantillasDocumento
            migrationBuilder.DropColumn(name: "CreadoPor", table: "nom_PlantillasDocumento");
            migrationBuilder.DropColumn(name: "FechaModificacion", table: "nom_PlantillasDocumento");
            migrationBuilder.DropColumn(name: "ModificadoPor", table: "nom_PlantillasDocumento");
        }
    }
}
