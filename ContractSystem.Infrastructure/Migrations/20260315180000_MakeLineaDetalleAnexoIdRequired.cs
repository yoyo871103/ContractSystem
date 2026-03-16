using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ContractSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeLineaDetalleAnexoIdRequired : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

            // Delete orphan lines without AnexoId (shouldn't exist in practice)
            migrationBuilder.Sql("DELETE FROM LineasDetalle WHERE AnexoId IS NULL;");

            if (!isSqlite)
            {
                // Drop existing FK and index
                migrationBuilder.DropForeignKey("FK_LineasDetalle_Anexos_AnexoId", "LineasDetalle");
                migrationBuilder.DropIndex("IX_LineasDetalle_AnexoId", "LineasDetalle");

                // Alter column to NOT NULL
                migrationBuilder.AlterColumn<int>(
                    name: "AnexoId",
                    table: "LineasDetalle",
                    type: "int",
                    nullable: false,
                    oldClrType: typeof(int),
                    oldType: "int",
                    oldNullable: true);

                // Recreate index and FK
                migrationBuilder.CreateIndex("IX_LineasDetalle_AnexoId", "LineasDetalle", "AnexoId");
                migrationBuilder.AddForeignKey(
                    name: "FK_LineasDetalle_Anexos_AnexoId",
                    table: "LineasDetalle",
                    column: "AnexoId",
                    principalTable: "Anexos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            }
            // SQLite doesn't support ALTER COLUMN; since tables were just created, no action needed
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var isSqlite = migrationBuilder.ActiveProvider?.Contains("Sqlite", StringComparison.OrdinalIgnoreCase) == true;

            if (!isSqlite)
            {
                migrationBuilder.DropForeignKey("FK_LineasDetalle_Anexos_AnexoId", "LineasDetalle");
                migrationBuilder.DropIndex("IX_LineasDetalle_AnexoId", "LineasDetalle");

                migrationBuilder.AlterColumn<int>(
                    name: "AnexoId",
                    table: "LineasDetalle",
                    type: "int",
                    nullable: true,
                    oldClrType: typeof(int),
                    oldType: "int");

                migrationBuilder.CreateIndex("IX_LineasDetalle_AnexoId", "LineasDetalle", "AnexoId");
                migrationBuilder.AddForeignKey(
                    name: "FK_LineasDetalle_Anexos_AnexoId",
                    table: "LineasDetalle",
                    column: "AnexoId",
                    principalTable: "Anexos",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            }
        }
    }
}
