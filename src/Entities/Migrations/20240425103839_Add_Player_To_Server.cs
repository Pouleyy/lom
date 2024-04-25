using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Add_Player_To_Server : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "guild_id",
                table: "players",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddColumn<long>(
                name: "server_id",
                table: "players",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_players_server_id",
                table: "players",
                column: "server_id");

            migrationBuilder.AddForeignKey(
                name: "FK_players_servers_server_id",
                table: "players",
                column: "server_id",
                principalTable: "servers",
                principalColumn: "server_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_players_servers_server_id",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_server_id",
                table: "players");

            migrationBuilder.DropColumn(
                name: "server_id",
                table: "players");

            migrationBuilder.AlterColumn<long>(
                name: "guild_id",
                table: "players",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);
        }
    }
}
