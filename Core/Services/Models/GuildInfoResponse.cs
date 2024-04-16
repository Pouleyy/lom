using System.Text.Json.Serialization;

namespace Core.Services.Models;

public class GuildInfoResponse
{
    [JsonPropertyName("guild_info")]
    public GuildInfo GuildInfo { get; set; } = new GuildInfo();
}

public class GuildInfo
{
    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; set; }
    [JsonPropertyName("name")]
    public string GuildName { get; set; } = string.Empty;
    [JsonPropertyName("notice")]
    public string Notice { get; set; } = string.Empty;
    [JsonPropertyName("leader_id")]
    public ulong LeaderId { get; set; }
    [JsonPropertyName("level")]
    public int Level { get; set; }
    [JsonPropertyName("create_time")]
    public ulong CreateTime { get; set; }
}