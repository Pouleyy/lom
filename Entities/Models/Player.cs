using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("players")]
public class Player
{
    [Column("player_id")]
    public ulong PlayerId { get; set; }
    
    [Column("player_name")]
    public string PlayerName { get; set; } = "";
    
    [Column("profile_picture_url")]
    public string ProfilePictureUrl { get; set; } = "";
    
    [Column("power")]
    public int Power { get; set; }
    
    [Column("attack")]
    public int Attack { get; set; }
    
    [Column("defense")]
    public int Defense { get; set; }
    
    [Column("health")]
    public int Health { get; set; }
    
    [Column("role")]
    public Role Role { get; set; }
    
    [Column("donation_weekly")]
    public int DonationWeekly { get; set; }
    
    [Column("last_login")]
    public DateTime LastLogin { get; set; }
    
    [Column("guild_id")]
    public ulong GuildId { get; set; }
    
    public virtual Family Family { get; set; } = null!;
    
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Player>()
                    .HasKey(p => p.PlayerId)
                    .HasName("players_pkey");
        
        modelBuilder.Entity<Player>()
                    .Property(p => p.PlayerId)
                    .ValueGeneratedNever();
        
        modelBuilder.Entity<Player>()
                    .Property(p => p.Role)
                    .HasConversion<int>();
        
        modelBuilder.Entity<Player>()
            .HasOne(p => p.Family)
            .WithMany(f => f.Players)
            .HasForeignKey(p => p.GuildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}