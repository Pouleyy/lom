using Core.Hangfire.Interfaces;
using Core.Services.Interface;
using Entities.Context;
using Entities.Models;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Server = Entities.Models.Server;

namespace Jobs.Jobs;

public class ServerScrapperJob(LomDbContext lomDbContext, IJoyNetClient joyNetClient, ILogger<ServerScrapperJob> logger) : IServerScrapperJob
{
    public async Task ExecuteAsync(PerformContext context, CancellationToken cancellationToken = default)
    {
        var dbServers = await lomDbContext.Servers.ToDictionaryAsync(x => x.ServerId, cancellationToken: cancellationToken);
        var euServers = await joyNetClient.GetEuServers(cancellationToken);
        var estServers = await joyNetClient.GetEstServers(cancellationToken);
        logger.LogInformation("Got {EuServersCount} EU servers and {EstServersCount} EST servers", euServers.Count, estServers.Count);
        logger.LogInformation("Got {DbServersCount} servers in database", dbServers.Count);
        logger.LogInformation("Got {TotalServersCount} total servers", euServers.Count + estServers.Count);
        //Merge two lists but keep trace of the region
        var servers = euServers.Concat(estServers).Distinct().ToList();
        var progress = context.WriteProgressBar();
        foreach (var server in servers.WithProgress(progress))
        {
            if (dbServers.ContainsKey(server.ServerId)) continue;
            var dbServer = new Server
            {
                ServerId = server.ServerId,
                ServerName = server.Name,
                Region = Enum.Parse<Region>(server.Region),
                OpenedTime = server.OpenedTime,
                SubRegion = Enum.Parse<SubRegion>(server.Name.Split('-')[0])
            };
            await lomDbContext.Servers.AddAsync(dbServer, cancellationToken);
        }
        await lomDbContext.SaveChangesAsync(cancellationToken);
    }
}