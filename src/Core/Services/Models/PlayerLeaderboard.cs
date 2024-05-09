using Entities.Models;

namespace Core.Services.Models;

public class PlayerLeaderboard
{
    public string Name { get; set; } = string.Empty;
    public long Power { get; set; }
    public int ServerId { get; set; }
    public SubRegion SubRegion { get; set; }
}