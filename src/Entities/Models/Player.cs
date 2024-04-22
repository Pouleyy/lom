using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("players")]
public class Player
{
    [Column("player_id", TypeName = "bigint")]
    public ulong PlayerId { get; set; }
    
    [Column("uid")]
    public string Uid { get; set; } = "";
    
    [Column("player_name")]
    public string PlayerName { get; set; } = "";
    
    [Column("profile_picture_url")]
    public string ProfilePictureUrl { get; set; } = "";
    
    [Column("level")]
    public int Level { get; set; }
    
    [Column("power", TypeName = "bigint")]
    public ulong Power { get; set; }
    
    [Column("attack", TypeName = "bigint")]
    public ulong Attack { get; set; }
    
    [Column("defense", TypeName = "bigint")]
    public ulong Defense { get; set; }
    
    [Column("health", TypeName = "bigint")]
    public ulong Health { get; set; }
    
    [Column("crit_rate")]
    public int CritRate { get; set; }
    
    [Column("crit_multiplier")]
    public int CritMultiplier { get; set; }
    
    [Column("crit_res")]
    public int CritRes { get; set; }
    
    [Column("evasion")]
    public int Evasion { get; set; }
    
    [Column("combo")]
    public int Combo { get; set; }
    
    [Column("counterstrike")]
    public int Counterstrike { get; set; }
    
    [Column("stun")]
    public int Stun { get; set; }
    
    [Column("combo_multiplier")]
    public int ComboMultiplier { get; set; }
    
    [Column("counterstrike_multiplier")]
    public int CounterstrikeMultiplier { get; set; }
    
    [Column("role")]
    public Role Role { get; set; }
    
    [Column("donation_weekly")]
    public int DonationWeekly { get; set; }
    
    [Column("last_login")]
    public DateTime LastLogin { get; set; }
    
    [Column("last_update")]
    public DateTime LastUpdate { get; set; }
    
    [Column("guild_id", TypeName = "bigint")]
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
        
        modelBuilder.Entity<Player>()
                    .HasIndex(p => p.Uid);
        
        modelBuilder.Entity<Player>()
                    .HasIndex(p => p.Power);
    }
}