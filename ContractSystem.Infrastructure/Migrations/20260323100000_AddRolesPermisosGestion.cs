using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRolesPermisosGestion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var stringType = isSqlite ? "TEXT" : "nvarchar(256)";
            var string64Type = isSqlite ? "TEXT" : "nvarchar(64)";
            var intType = isSqlite ? "INTEGER" : "int";

            // 1. Agregar columna Categoria a Permisos
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Permisos",
                type: string64Type,
                maxLength: 64,
                nullable: true);

            // 2. Agregar columna Descripcion a Roles
            migrationBuilder.AddColumn<string>(
                name: "Descripcion",
                table: "Roles",
                type: stringType,
                maxLength: 256,
                nullable: true);

            // 3. Crear tabla UsuarioPermisos (permisos directos sin rol)
            migrationBuilder.CreateTable(
                name: "UsuarioPermisos",
                columns: table => new
                {
                    UsuarioId = table.Column<int>(type: intType, nullable: false),
                    PermisoId = table.Column<int>(type: intType, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsuarioPermisos", x => new { x.UsuarioId, x.PermisoId });
                    table.ForeignKey(
                        name: "FK_UsuarioPermisos_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UsuarioPermisos_Permisos_PermisoId",
                        column: x => x.PermisoId,
                        principalTable: "Permisos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UsuarioPermisos_PermisoId",
                table: "UsuarioPermisos",
                column: "PermisoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "UsuarioPermisos");
            migrationBuilder.DropColumn(name: "Descripcion", table: "Roles");
            migrationBuilder.DropColumn(name: "Categoria", table: "Permisos");
        }
    }
}
