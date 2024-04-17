using System.Text.Json.Serialization;

namespace Core.Services.Models;

public class RoleOthersResponse
{
    [JsonPropertyName("other_role_info")] 
    public OtherRoleInfo[] Players { get; set; } = [];
}

public class OtherRoleInfo
{
    [JsonPropertyName("role_id")] 
    public ulong PlayerId { get; set; }
    [JsonPropertyName("info_list")] 
    public InfoList InfoList { get; set; } = new();
    [JsonPropertyName("sp_list")]
    public List<Kv> SpList { get; set; } = [];
}

public class InfoList
{
    [JsonPropertyName("kv")]
    public List<Kv> KeyValues { get; set; } = [];
}

public class Kv
{
    [JsonPropertyName("k")]
    public int Key { get; set; }
    [JsonPropertyName("v")]
    public ulong Value { get; set; }
}