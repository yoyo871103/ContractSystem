using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnexosYLineasDetalle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var decimalType4 = isSqlite ? "TEXT" : "decimal(18,4)";
            var decimalType2 = isSqlite ? "TEXT" : "decimal(18,2)";
            var bitType = isSqlite ? "INTEGER" : "bit";

            migrationBuilder.CreateTable(
                name: "Anexos",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    ContratoId = table.Column<int>(type: intType, nullable: false),
                    Nombre = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    CondicionesEntrega = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CostosAsociados = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Orden = table.Column<int>(type: intType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anexos", x => x.Id);
                    table.ForeignKey("FK_Anexos_Contratos_ContratoId", x => x.ContratoId, "Contratos", "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LineasDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    ContratoId = table.Column<int>(type: intType, nullable: false),
                    AnexoId = table.Column<int>(type: intType, nullable: true),
                    Tipo = table.Column<int>(type: intType, nullable: false),
                    Concepto = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(512)", maxLength: 512, nullable: false),
                    Descripcion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(2048)", maxLength: 2048, nullable: true),
                    Cantidad = table.Column<decimal>(type: decimalType4, nullable: false),
                    UnidadMedidaTexto = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(64)", maxLength: 64, nullable: true),
                    UnidadMedidaId = table.Column<int>(type: intType, nullable: true),
                    PrecioUnitario = table.Column<decimal>(type: decimalType4, nullable: false),
                    ImporteTotal = table.Column<decimal>(type: decimalType2, nullable: false),
                    ProductoServicioOrigenId = table.Column<int>(type: intType, nullable: true),
                    EsCopiaDeOriginal = table.Column<bool>(type: bitType, nullable: false),
                    Orden = table.Column<int>(type: intType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LineasDetalle", x => x.Id);
                    table.ForeignKey("FK_LineasDetalle_Contratos_ContratoId", x => x.ContratoId, "Contratos", "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey("FK_LineasDetalle_Anexos_AnexoId", x => x.AnexoId, "Anexos", "Id",
                        onDelete: isSqlite ? ReferentialAction.SetNull : ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex("IX_Anexos_ContratoId", "Anexos", "ContratoId");
            migrationBuilder.CreateIndex("IX_LineasDetalle_ContratoId", "LineasDetalle", "ContratoId");
            migrationBuilder.CreateIndex("IX_LineasDetalle_AnexoId", "LineasDetalle", "AnexoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "LineasDetalle");
            migrationBuilder.DropTable(name: "Anexos");
        }
    }
}
