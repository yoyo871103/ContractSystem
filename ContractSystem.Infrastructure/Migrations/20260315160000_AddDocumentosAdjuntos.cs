using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentosAdjuntos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var bigintType = isSqlite ? "INTEGER" : "bigint";
            var blobType = isSqlite ? "BLOB" : "varbinary(max)";

            migrationBuilder.CreateTable(
                name: "DocumentosAdjuntos",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    NombreArchivo = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(500)", maxLength: 500, nullable: false),
                    Extension = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(20)", maxLength: 20, nullable: false),
                    Objetivo = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Contenido = table.Column<byte[]>(type: blobType, nullable: false),
                    TamanioBytes = table.Column<long>(type: bigintType, nullable: false),
                    FechaCarga = table.Column<DateTime>(type: isSqlite ? "TEXT" : "datetime2", nullable: false),
                    UsuarioCargaId = table.Column<int>(type: intType, nullable: true),
                    ContratoId = table.Column<int>(type: intType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosAdjuntos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosAdjuntos_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosAdjuntos_ContratoId",
                table: "DocumentosAdjuntos",
                column: "ContratoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "DocumentosAdjuntos");
        }
    }
}
