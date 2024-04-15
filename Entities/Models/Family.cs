using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("families")]
public class Family
{
    [Column("guild_id")]
    public ulong GuildId { get; set; }
    
    [Column("guild_name")]
    public string GuildName { get; set; } = "";
    
    [Column("notice")]
    public string Notice { get; set; } = "";
    
    [Column("created_time")]
    public DateTime CreatedTime { get; set; }
    
    [Column("level")]
    public int Level { get; set; }
    
    [Column("leader_id")]
    public ulong LeaderId { get; set; }
    
    [Column("server_id")]
    public int ServerId { get; set; }
    
    public virtual Server Server { get; set; } = null!;
    
    public virtual ICollection<Player> Players { get; set; } = new List<Player>();
    
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Family>()
                    .HasKey(f => f.GuildId)
                    .HasName("families_pkey");
        
        modelBuilder.Entity<Family>()
                    .Property(f => f.GuildId)
                    .ValueGeneratedNever();
        
        modelBuilder.Entity<Family>()
            .HasOne(f => f.Server)
            .WithMany(s => s.Families)
            .HasForeignKey(f => f.ServerId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}