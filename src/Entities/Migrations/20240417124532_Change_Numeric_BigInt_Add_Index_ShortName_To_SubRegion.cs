using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Change_Numeric_BigInt_Add_Index_ShortName_To_SubRegion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<long>(
                name: "min_guild_id",
                table: "servers",
                type: "bigint",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "power",
                table: "players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "health",
                table: "players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "guild_id",
                table: "players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "defense",
                table: "players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "attack",
                table: "players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "player_id",
                table: "players",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "leader_id",
                table: "families",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.AlterColumn<long>(
                name: "guild_id",
                table: "families",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(20,0)");

            migrationBuilder.CreateIndex(
                name: "IX_servers_server_name",
                table: "servers",
                column: "server_name");

            migrationBuilder.CreateIndex(
                name: "IX_servers_short_name",
                table: "servers",
                column: "short_name");

            migrationBuilder.CreateIndex(
                name: "IX_players_power",
                table: "players",
                column: "power");

            migrationBuilder.CreateIndex(
                name: "IX_players_uid",
                table: "players",
                column: "uid");

            migrationBuilder.CreateIndex(
                name: "IX_families_guild_name",
                table: "families",
                column: "guild_name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_servers_server_name",
                table: "servers");

            migrationBuilder.DropIndex(
                name: "IX_servers_short_name",
                table: "servers");

            migrationBuilder.DropIndex(
                name: "IX_players_power",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_players_uid",
                table: "players");

            migrationBuilder.DropIndex(
                name: "IX_families_guild_name",
                table: "families");

            migrationBuilder.AlterColumn<decimal>(
                name: "min_guild_id",
                table: "servers",
                type: "numeric(20,0)",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "bigint",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "power",
                table: "players",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "health",
                table: "players",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "guild_id",
                table: "players",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "defense",
                table: "players",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<int>(
                name: "attack",
                table: "players",
                type: "integer",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "player_id",
                table: "players",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "leader_id",
                table: "families",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AlterColumn<decimal>(
                name: "guild_id",
                table: "families",
                type: "numeric(20,0)",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");
        }
    }
}
