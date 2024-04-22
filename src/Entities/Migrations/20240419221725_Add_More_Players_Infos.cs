using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Add_More_Players_Infos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "combo",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "combo_multiplier",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "counterstrike",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "counterstrike_multiplier",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "crit_multiplier",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "crit_rate",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "crit_res",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "evasion",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "level",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "stun",
                table: "players",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "combo",
                table: "players");

            migrationBuilder.DropColumn(
                name: "combo_multiplier",
                table: "players");

            migrationBuilder.DropColumn(
                name: "counterstrike",
                table: "players");

            migrationBuilder.DropColumn(
                name: "counterstrike_multiplier",
                table: "players");

            migrationBuilder.DropColumn(
                name: "crit_multiplier",
                table: "players");

            migrationBuilder.DropColumn(
                name: "crit_rate",
                table: "players");

            migrationBuilder.DropColumn(
                name: "crit_res",
                table: "players");

            migrationBuilder.DropColumn(
                name: "evasion",
                table: "players");

            migrationBuilder.DropColumn(
                name: "level",
                table: "players");

            migrationBuilder.DropColumn(
                name: "stun",
                table: "players");
        }
    }
}
