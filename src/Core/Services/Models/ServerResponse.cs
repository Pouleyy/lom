using System.Text.Json.Serialization;

namespace Core.Services.Models;

public class ServerResponse
{
    [JsonPropertyName("server_list")] 
    public IList<Server> Servers { get; set; } = [];
}

public class Server
{
    public int ServerId
    {
        get
        {
            int.TryParse(Id, out var id);
            return id;
        }
    }
    
    [JsonInclude]
    private string Id { get; set; } = "";
    
    public string Name { get; set; } = "";

    public DateTime OpenedTime => DateTimeOffset.FromUnixTimeSeconds(OpenTime).UtcDateTime;

    [JsonPropertyName("opentime")]
    [JsonInclude]
    private long OpenTime { get; set; }
    
    public string Region { get; set; } = "";

    public int? MergedInto
    {
        get
        {
            int.TryParse(MergeId, out var id);
            return id == 0 ? null : id;
        }    
    }
    
    [JsonPropertyName("merge_id")]
    public string MergeId { get; set; }
}