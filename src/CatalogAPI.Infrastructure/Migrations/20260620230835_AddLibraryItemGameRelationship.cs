using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalogAPI.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLibraryItemGameRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_library_game_id",
                table: "library",
                column: "game_id");

            migrationBuilder.AddForeignKey(
                name: "FK_library_games_game_id",
                table: "library",
                column: "game_id",
                principalTable: "games",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_library_games_game_id",
                table: "library");

            migrationBuilder.DropIndex(
                name: "IX_library_game_id",
                table: "library");
        }
    }
}
