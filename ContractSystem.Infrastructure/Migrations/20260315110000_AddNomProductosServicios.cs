using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddNomProductosServicios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var dateType = isSqlite ? "TEXT" : "datetimeoffset";
            var decimalType = isSqlite ? "TEXT" : "decimal(18,2)";

            migrationBuilder.CreateTable(
                name: "nom_ProductosServicios",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Codigo = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(64)", maxLength: 64, nullable: true),
                    Nombre = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    Descripcion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(1024)", maxLength: 1024, nullable: false),
                    Tipo = table.Column<int>(type: intType, nullable: false),
                    UnidadMedidaId = table.Column<int>(type: intType, nullable: true),
                    PrecioEstimado = table.Column<decimal>(type: decimalType, nullable: true),
                    DeletedAt = table.Column<DateTimeOffset?>(type: dateType, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nom_ProductosServicios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_nom_ProductosServicios_nom_UnidadesMedida_UnidadMedidaId",
                        column: x => x.UnidadMedidaId,
                        principalTable: "nom_UnidadesMedida",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_nom_ProductosServicios_Codigo",
                table: "nom_ProductosServicios",
                column: "Codigo");

            migrationBuilder.CreateIndex(
                name: "IX_nom_ProductosServicios_Tipo",
                table: "nom_ProductosServicios",
                column: "Tipo");

            migrationBuilder.CreateIndex(
                name: "IX_nom_ProductosServicios_UnidadMedidaId",
                table: "nom_ProductosServicios",
                column: "UnidadMedidaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "nom_ProductosServicios");
        }
    }
}
