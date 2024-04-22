using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Entities.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "servers",
                columns: table => new
                {
                    server_id = table.Column<int>(type: "integer", nullable: false),
                    server_name = table.Column<string>(type: "text", nullable: false),
                    region = table.Column<int>(type: "integer", nullable: false),
                    short_name = table.Column<string>(type: "text", nullable: false),
                    opened_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("servers_pkey", x => x.server_id);
                });

            migrationBuilder.CreateTable(
                name: "families",
                columns: table => new
                {
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    guild_name = table.Column<string>(type: "text", nullable: false),
                    notice = table.Column<string>(type: "text", nullable: false),
                    created_time = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    level = table.Column<int>(type: "integer", nullable: false),
                    leader_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    server_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("families_pkey", x => x.guild_id);
                    table.ForeignKey(
                        name: "FK_families_servers_server_id",
                        column: x => x.server_id,
                        principalTable: "servers",
                        principalColumn: "server_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "players",
                columns: table => new
                {
                    player_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false),
                    player_name = table.Column<string>(type: "text", nullable: false),
                    profile_picture_url = table.Column<string>(type: "text", nullable: false),
                    power = table.Column<int>(type: "integer", nullable: false),
                    attack = table.Column<int>(type: "integer", nullable: false),
                    defense = table.Column<int>(type: "integer", nullable: false),
                    health = table.Column<int>(type: "integer", nullable: false),
                    role = table.Column<int>(type: "integer", nullable: false),
                    donation_weekly = table.Column<int>(type: "integer", nullable: false),
                    last_login = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    guild_id = table.Column<decimal>(type: "numeric(20,0)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("players_pkey", x => x.player_id);
                    table.ForeignKey(
                        name: "FK_players_families_guild_id",
                        column: x => x.guild_id,
                        principalTable: "families",
                        principalColumn: "guild_id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_families_server_id",
                table: "families",
                column: "server_id");

            migrationBuilder.CreateIndex(
                name: "IX_players_guild_id",
                table: "players",
                column: "guild_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "players");

            migrationBuilder.DropTable(
                name: "families");

            migrationBuilder.DropTable(
                name: "servers");
        }
    }
}
