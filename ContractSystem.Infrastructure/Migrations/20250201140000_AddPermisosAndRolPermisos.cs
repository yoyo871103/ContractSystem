using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPermisosAndRolPermisos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var textType = isSqlite ? "TEXT" : "nvarchar";

            migrationBuilder.CreateTable(
                name: "Permisos",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Nombre = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(64)", maxLength: 64, nullable: false),
                    Descripcion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permisos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RolPermisos",
                columns: table => new
                {
                    RolId = table.Column<int>(type: intType, nullable: false),
                    PermisoId = table.Column<int>(type: intType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolPermisos", x => new { x.RolId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_RolPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolPermisos_Roles_RolId",
                        column: x => x.RolId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Permisos_Nombre",
                table: "Permisos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolPermisos_PermisoId",
                table: "RolPermisos",
                column: "PermisoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "RolPermisos");
            migrationBuilder.DropTable(name: "Permisos");
        }
    }
}
