using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <summary>
    /// Datos legales para llenar contratos:
    /// - BusinessInfos / nom_Terceros: CodigoReeup, RegistroMercantil, DocumentoConstitucion.
    /// - RepresentantesLegales y CuentasBancarias: pertenecen a la entidad propia o a un tercero.
    /// </summary>
    public partial class AddDatosLegales : Migration
    {
        private static readonly string[] TablasConDatosLegales = { "BusinessInfos", "nom_Terceros" };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var boolType = isSqlite ? "INTEGER" : "bit";
            var dateType = isSqlite ? "TEXT" : "datetime2";
            string Text(int len) => isSqlite ? "TEXT" : $"nvarchar({len})";

            foreach (var tabla in TablasConDatosLegales)
            {
                migrationBuilder.AddColumn<string>(name: "CodigoReeup", table: tabla, type: Text(32), maxLength: 32, nullable: true);
                migrationBuilder.AddColumn<string>(name: "RegistroMercantil", table: tabla, type: Text(256), maxLength: 256, nullable: true);
                migrationBuilder.AddColumn<string>(name: "DocumentoConstitucion", table: tabla, type: Text(512), maxLength: 512, nullable: true);
            }

            migrationBuilder.CreateTable(
                name: "RepresentantesLegales",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    BusinessInfoId = table.Column<int>(type: intType, nullable: true),
                    TerceroId = table.Column<int>(type: intType, nullable: true),
                    Nombre = table.Column<string>(type: Text(256), maxLength: 256, nullable: false),
                    Cargo = table.Column<string>(type: Text(128), maxLength: 128, nullable: false),
                    CarneIdentidad = table.Column<string>(type: Text(32), maxLength: 32, nullable: true),
                    DocumentoNombramiento = table.Column<string>(type: Text(512), maxLength: 512, nullable: true),
                    FechaNombramiento = table.Column<DateTime>(type: dateType, nullable: true),
                    EsPredeterminado = table.Column<bool>(type: boolType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RepresentantesLegales", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RepresentantesLegales_BusinessInfos_BusinessInfoId",
                        column: x => x.BusinessInfoId,
                        principalTable: "BusinessInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RepresentantesLegales_nom_Terceros_TerceroId",
                        column: x => x.TerceroId,
                        principalTable: "nom_Terceros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CuentasBancarias",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    BusinessInfoId = table.Column<int>(type: intType, nullable: true),
                    TerceroId = table.Column<int>(type: intType, nullable: true),
                    Numero = table.Column<string>(type: Text(64), maxLength: 64, nullable: false),
                    Moneda = table.Column<string>(type: Text(10), maxLength: 10, nullable: false),
                    Banco = table.Column<string>(type: Text(128), maxLength: 128, nullable: false),
                    Sucursal = table.Column<string>(type: Text(128), maxLength: 128, nullable: true),
                    DireccionSucursal = table.Column<string>(type: Text(256), maxLength: 256, nullable: true),
                    Titular = table.Column<string>(type: Text(256), maxLength: 256, nullable: true),
                    EsPredeterminada = table.Column<bool>(type: boolType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CuentasBancarias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_BusinessInfos_BusinessInfoId",
                        column: x => x.BusinessInfoId,
                        principalTable: "BusinessInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CuentasBancarias_nom_Terceros_TerceroId",
                        column: x => x.TerceroId,
                        principalTable: "nom_Terceros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(name: "IX_RepresentantesLegales_BusinessInfoId", table: "RepresentantesLegales", column: "BusinessInfoId");
            migrationBuilder.CreateIndex(name: "IX_RepresentantesLegales_TerceroId", table: "RepresentantesLegales", column: "TerceroId");
            migrationBuilder.CreateIndex(name: "IX_CuentasBancarias_BusinessInfoId", table: "CuentasBancarias", column: "BusinessInfoId");
            migrationBuilder.CreateIndex(name: "IX_CuentasBancarias_TerceroId", table: "CuentasBancarias", column: "TerceroId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "CuentasBancarias");
            migrationBuilder.DropTable(name: "RepresentantesLegales");

            foreach (var tabla in TablasConDatosLegales)
            {
                migrationBuilder.DropColumn(name: "CodigoReeup", table: tabla);
                migrationBuilder.DropColumn(name: "RegistroMercantil", table: tabla);
                migrationBuilder.DropColumn(name: "DocumentoConstitucion", table: tabla);
            }
        }
    }
}
