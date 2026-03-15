using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioProfileAndSoftDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var dateType = isSqlite ? "TEXT" : "datetimeoffset";

            migrationBuilder.AddColumn<DateTimeOffset?>(
                name: "DeletedAt",
                table: "Usuarios",
                type: dateType,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Usuarios",
                type: isSqlite ? "TEXT" : "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "FotoPerfil",
                table: "Usuarios",
                type: isSqlite ? "BLOB" : "varbinary(max)",
                maxLength: 524288,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(name: "DeletedAt", table: "Usuarios");
            migrationBuilder.DropColumn(name: "Email", table: "Usuarios");
            migrationBuilder.DropColumn(name: "FotoPerfil", table: "Usuarios");
        }
    }
}
