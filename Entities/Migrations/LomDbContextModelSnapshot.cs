﻿// <auto-generated />
using System;
using Entities.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Entities.Migrations
{
    [DbContext(typeof(LomDbContext))]
    partial class LomDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.4")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Entities.Models.Family", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<DateTime>("CreatedTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("created_time");

                    b.Property<string>("GuildName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("guild_name");

                    b.Property<decimal>("LeaderId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("leader_id");

                    b.Property<int>("Level")
                        .HasColumnType("integer")
                        .HasColumnName("level");

                    b.Property<string>("Notice")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("notice");

                    b.Property<int>("ServerId")
                        .HasColumnType("integer")
                        .HasColumnName("server_id");

                    b.HasKey("GuildId")
                        .HasName("families_pkey");

                    b.HasIndex("ServerId");

                    b.ToTable("families");
                });

            modelBuilder.Entity("Entities.Models.Player", b =>
                {
                    b.Property<decimal>("PlayerId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("player_id");

                    b.Property<int>("Attack")
                        .HasColumnType("integer")
                        .HasColumnName("attack");

                    b.Property<int>("Defense")
                        .HasColumnType("integer")
                        .HasColumnName("defense");

                    b.Property<int>("DonationWeekly")
                        .HasColumnType("integer")
                        .HasColumnName("donation_weekly");

                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)")
                        .HasColumnName("guild_id");

                    b.Property<int>("Health")
                        .HasColumnType("integer")
                        .HasColumnName("health");

                    b.Property<DateTime>("LastLogin")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("last_login");

                    b.Property<string>("PlayerName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("player_name");

                    b.Property<int>("Power")
                        .HasColumnType("integer")
                        .HasColumnName("power");

                    b.Property<string>("ProfilePictureUrl")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("profile_picture_url");

                    b.Property<int>("Role")
                        .HasColumnType("integer")
                        .HasColumnName("role");

                    b.HasKey("PlayerId")
                        .HasName("players_pkey");

                    b.HasIndex("GuildId");

                    b.ToTable("players");
                });

            modelBuilder.Entity("Entities.Models.Server", b =>
                {
                    b.Property<int>("ServerId")
                        .HasColumnType("integer")
                        .HasColumnName("server_id");

                    b.Property<DateTime>("OpenedTime")
                        .HasColumnType("timestamp with time zone")
                        .HasColumnName("opened_time");

                    b.Property<int>("Region")
                        .HasColumnType("integer")
                        .HasColumnName("region");

                    b.Property<string>("ServerName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("server_name");

                    b.Property<string>("ShortName")
                        .IsRequired()
                        .HasColumnType("text")
                        .HasColumnName("short_name");

                    b.HasKey("ServerId")
                        .HasName("servers_pkey");

                    b.ToTable("servers");
                });

            modelBuilder.Entity("Entities.Models.Family", b =>
                {
                    b.HasOne("Entities.Models.Server", "Server")
                        .WithMany("Families")
                        .HasForeignKey("ServerId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Server");
                });

            modelBuilder.Entity("Entities.Models.Player", b =>
                {
                    b.HasOne("Entities.Models.Family", "Family")
                        .WithMany("Players")
                        .HasForeignKey("GuildId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Family");
                });

            modelBuilder.Entity("Entities.Models.Family", b =>
                {
                    b.Navigation("Players");
                });

            modelBuilder.Entity("Entities.Models.Server", b =>
                {
                    b.Navigation("Families");
                });
#pragma warning restore 612, 618
        }
    }
}
