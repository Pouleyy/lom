using Core.Services.Interface;
using Core.Services.Models;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Core.Services;

public class JoyNetClient(HttpClient httpClient, ILogger<IJoyNetClient> logger) : IJoyNetClient
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
    
    public async Task<IList<Server>> GetEstServers(CancellationToken cancellationToken)
    {
        var url = "https://slogin1001-mix-us-xxjzz.joynetgamestudio.com/client/server_list?time=1713134996&uid=&plat=4012&ticket=9facdf411176aefdd7c4fb2119883127\n";
        var responseMessage = await httpClient.GetAsync(url, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
        {
            logger.LogError($"Error making request to {GetType().Name} API {{ResponseMessageStatusCode}} {{ResponseMessageReasonPhrase}} to {{Url}}",
                             responseMessage.StatusCode,
                             responseMessage.ReasonPhrase,
                             url);
            
            return default!;
        }
        await using var content = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
        return (await JsonSerializer.DeserializeAsync<ServerResponse>(content, _jsonSerializerOptions, cancellationToken))!.Servers.Select(x =>
        {
            x.Region = "EST";
            return x;
        }).ToList();
    }

    public async Task<IList<Server>> GetEuServers(CancellationToken cancellationToken)
    {
        var url = "https://slogin1001-mix-us2-xxjzz.joynetgamestudio.com/client/server_list?time=1713134996&uid=&plat=4012&ticket=9facdf411176aefdd7c4fb2119883127";
        var responseMessage = await httpClient.GetAsync(url, cancellationToken);
        if (!responseMessage.IsSuccessStatusCode)
        {
            logger.LogError($"Error making request to {GetType().Name} API {{ResponseMessageStatusCode}} {{ResponseMessageReasonPhrase}} to {{Url}}",
                             responseMessage.StatusCode,
                             responseMessage.ReasonPhrase,
                             url);
            
            return default!;
        }
        await using var content = await responseMessage.Content.ReadAsStreamAsync(cancellationToken);
        return (await JsonSerializer.DeserializeAsync<ServerResponse>(content, _jsonSerializerOptions, cancellationToken))!.Servers.Select(x =>
        {
            x.Region = "EU";
            return x;
        }).ToList();
    }
}