using Entities.Helper;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Entities.Models;

[Table("servers")]
public class Server
{
    [Column("server_id")]
    public int ServerId { get; set; }
    
    [Column("server_name")]
    public string ServerName { get; set; } = "";
    
    [Column("region")]
    public Region Region { get; set; }
    
    [Column("short_name")]
    public ServerShortName ShortName { get; set; }
    
    [Column("opened_time")]
    public DateTime OpenedTime { get; set; }
    
    [Column("min_guild_id")]
    public long? MinGuildId { get; set; }
    
    public virtual ICollection<Family> Families { get; set; } = new List<Family>();
    
    public static void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Server>()
                    .HasKey(s => s.ServerId)
                    .HasName("servers_pkey");
        
        modelBuilder.Entity<Server>()
                    .Property(s => s.ServerId)
                    .ValueGeneratedNever();
        
        modelBuilder.Entity<Server>()
                    .Property(s => s.Region)
                    .HasConversion<int>();

        modelBuilder.Entity<Server>()
                    .Property(s => s.ShortName)
                    .HasConversion(new ServerShortNameConverter());
    }
}