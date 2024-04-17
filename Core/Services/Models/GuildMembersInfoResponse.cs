using System.Text.Json.Serialization;

namespace Core.Services.Models;

public class GuildMembersInfoResponse
{
    [JsonPropertyName("guild_id")]
    public ulong GuildId { get; set; }
    [JsonPropertyName("member_list")]
    public List<MemberList> MemberList { get; set; } = [];
}

public class MemberList
{
    [JsonPropertyName("career")]
    public int Role { get; set; }
    [JsonPropertyName("donate_week")]
    public int DonateWeek { get; set; }
    [JsonPropertyName("is_online")]
    public int IsOnline { get; set; }
    [JsonPropertyName("offline_time")]
    public int LastLogin { get; set; }
    [JsonPropertyName("role_head")]
    public RoleHead RoleHead { get; set; } = new RoleHead();
    [JsonPropertyName("role_id")]
    public ulong PlayerId { get; set; }
    [JsonPropertyName("role_name")]
    public string PlayerName { get; set; } = string.Empty;
}

public class RoleHead
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    [JsonPropertyName("frame_id")]
    public int FrameId { get; set; }
    [JsonPropertyName("url")]
    public string Url { get; set; } = string.Empty;
}