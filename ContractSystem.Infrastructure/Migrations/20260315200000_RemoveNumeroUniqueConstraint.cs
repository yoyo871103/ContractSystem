using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveNumeroUniqueConstraint : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite";

            if (isSqlite)
            {
                // SQLite doesn't support DROP INDEX IF EXISTS natively, use raw SQL
                migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Contratos_Numero;");
                migrationBuilder.Sql("CREATE INDEX IX_Contratos_Numero ON Contratos (Numero);");
            }
            else
            {
                migrationBuilder.DropIndex(name: "IX_Contratos_Numero", table: "Contratos");
                migrationBuilder.CreateIndex(
                    name: "IX_Contratos_Numero",
                    table: "Contratos",
                    column: "Numero");
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider == "Microsoft.EntityFrameworkCore.Sqlite";

            if (isSqlite)
            {
                migrationBuilder.Sql("DROP INDEX IF EXISTS IX_Contratos_Numero;");
                migrationBuilder.Sql("CREATE UNIQUE INDEX IX_Contratos_Numero ON Contratos (Numero);");
            }
            else
            {
                migrationBuilder.DropIndex(name: "IX_Contratos_Numero", table: "Contratos");
                migrationBuilder.CreateIndex(
                    name: "IX_Contratos_Numero",
                    table: "Contratos",
                    column: "Numero",
                    unique: true);
            }
        }
    }
}
