using Core.Services.Models;

namespace Core.Services.Interface;

public interface IJoyNetClient
{
    Task<IList<Server>> GetEstServers(CancellationToken cancellationToken = default);
    Task<IList<Server>> GetEuServers(CancellationToken cancellationToken = default);
}