using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBusinessInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;
            var intType = isSqlite ? "INTEGER" : "int";
            var textType = isSqlite ? "TEXT" : "nvarchar";
            var blobType = isSqlite ? "BLOB" : "varbinary(max)";

            migrationBuilder.CreateTable(
                name: "BusinessInfos",
                columns: table => new
                {
                    Id = table.Column<int>(type: intType, nullable: false)
                        .Annotation(isSqlite ? "Sqlite:Autoincrement" : "SqlServer:Identity", isSqlite ? (object)true : "1, 1"),
                    Nombre = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    Logo = table.Column<byte[]>(type: blobType, nullable: true),
                    Nit = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(64)", maxLength: 64, nullable: false),
                    Direccion = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(512)", maxLength: 512, nullable: false),
                    Telefono = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(64)", maxLength: 64, nullable: false),
                    Email = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false),
                    Eslogan = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(512)", maxLength: 512, nullable: false),
                    NombreDueno = table.Column<string>(type: isSqlite ? "TEXT" : "nvarchar(256)", maxLength: 256, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BusinessInfos", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "BusinessInfos");
        }
    }
}
