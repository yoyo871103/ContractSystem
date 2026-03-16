using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContratosYModificaciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var dateType = isSqlite ? "TEXT" : "datetime2";
            var dateOffsetType = isSqlite ? "TEXT" : "datetimeoffset";
            var decimalType = isSqlite ? "TEXT" : "decimal(18,2)";
            var bitType = isSqlite ? "INTEGER" : "bit";

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Numero = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(128)", maxLength: 128, nullable: false),
                    Objeto = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(2048)", maxLength: 2048, nullable: false),
                    TipoDocumento = table.Column<int>(type: intType, nullable: false),
                    Rol = table.Column<int>(type: intType, nullable: false),
                    Estado = table.Column<int>(type: intType, nullable: false),
                    FechaFirma = table.Column<DateTime>(type: dateType, nullable: true),
                    FechaEntradaVigor = table.Column<DateTime>(type: dateType, nullable: true),
                    FechaVigencia = table.Column<DateTime>(type: dateType, nullable: true),
                    Duracion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: true),
                    Ejecutado = table.Column<bool>(type: bitType, nullable: false),
                    FechaEjecucion = table.Column<DateTime>(type: dateType, nullable: true),
                    MiEmpresaId = table.Column<int>(type: intType, nullable: true),
                    TerceroId = table.Column<int>(type: intType, nullable: true),
                    ContratoPadreId = table.Column<int>(type: intType, nullable: true),
                    ValorTotal = table.Column<decimal>(type: decimalType, nullable: true),
                    CondicionesEntrega = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CostosAsociados = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(4000)", maxLength: 4000, nullable: true),
                    EsModificacionGenerales = table.Column<bool>(type: bitType, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: dateType, nullable: false),
                    UsuarioCreacionId = table.Column<int>(type: intType, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset?>(type: dateOffsetType, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratos_BusinessInfos_MiEmpresaId",
                        column: x => x.MiEmpresaId,
                        principalTable: "BusinessInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Contratos_nom_Terceros_TerceroId",
                        column: x => x.TerceroId,
                        principalTable: "nom_Terceros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Contratos_Contratos_ContratoPadreId",
                        column: x => x.ContratoPadreId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ModificacionesDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    DocumentoOrigenId = table.Column<int>(type: intType, nullable: false),
                    DocumentoDestinoId = table.Column<int>(type: intType, nullable: false),
                    Descripcion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(2048)", maxLength: 2048, nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: dateType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModificacionesDocumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ModificacionesDocumento_Contratos_DocumentoOrigenId",
                        column: x => x.DocumentoOrigenId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ModificacionesDocumento_Contratos_DocumentoDestinoId",
                        column: x => x.DocumentoDestinoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Índices para Contratos
            migrationBuilder.CreateIndex(name: "IX_Contratos_Numero", table: "Contratos", column: "Numero", unique: true);
            migrationBuilder.CreateIndex(name: "IX_Contratos_TipoDocumento", table: "Contratos", column: "TipoDocumento");
            migrationBuilder.CreateIndex(name: "IX_Contratos_Estado", table: "Contratos", column: "Estado");
            migrationBuilder.CreateIndex(name: "IX_Contratos_Rol", table: "Contratos", column: "Rol");
            migrationBuilder.CreateIndex(name: "IX_Contratos_FechaFirma", table: "Contratos", column: "FechaFirma");
            migrationBuilder.CreateIndex(name: "IX_Contratos_FechaVigencia", table: "Contratos", column: "FechaVigencia");
            migrationBuilder.CreateIndex(name: "IX_Contratos_TerceroId", table: "Contratos", column: "TerceroId");
            migrationBuilder.CreateIndex(name: "IX_Contratos_ContratoPadreId", table: "Contratos", column: "ContratoPadreId");
            migrationBuilder.CreateIndex(name: "IX_Contratos_MiEmpresaId", table: "Contratos", column: "MiEmpresaId");

            // Índices para ModificacionesDocumento
            migrationBuilder.CreateIndex(
                name: "IX_ModificacionesDocumento_OrigenDestino",
                table: "ModificacionesDocumento",
                columns: new[] { "DocumentoOrigenId", "DocumentoDestinoId" },
                unique: true);
            migrationBuilder.CreateIndex(name: "IX_ModificacionesDocumento_DocumentoDestinoId", table: "ModificacionesDocumento", column: "DocumentoDestinoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "ModificacionesDocumento");
            migrationBuilder.DropTable(name: "Contratos");
        }
    }
}
