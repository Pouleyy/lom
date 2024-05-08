using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Add_Player_Class_Spouse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "class",
                table: "players",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "spouse_id",
                table: "players",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_spouse_id",
                table: "players",
                column: "spouse_id",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_players_players_spouse_id",
                table: "players",
                column: "spouse_id",
                principalTable: "players",
                principalColumn: "player_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_players_players_spouse_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_spouse_id",
                table: "players");

            migrationBuilder.DropColumn(
                name: "class",
                table: "players");

            migrationBuilder.DropColumn(
                name: "spouse_id",
                table: "players");
        }
    }
}
