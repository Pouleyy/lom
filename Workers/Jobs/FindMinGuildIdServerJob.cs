using Core.Hangfire.Interfaces;
using Core.Services;
using Core.Services.Models;
using Entities.Context;
using Entities.Models;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Server = Entities.Models.Server;

namespace Jobs.Jobs;

public class FindMinGuildIdServerJob(LomDbContext lomDbContext, BrowserService browserService, ILogger<FindMinGuildIdServerJob> logger) : IFindMinGuildIdServerJob
{
    private bool _guildFound;
    private ulong _minGuildId;

    public async Task ExecuteAsync(PerformContext context, SubRegion subRegion, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting find min guild id job for {SubRegion}", subRegion);
        var subRegionServers = await lomDbContext.Servers.OrderBy(x => x.ServerId).Where(x => x.SubRegion == subRegion).ToListAsync(cancellationToken: cancellationToken);
        if (subRegionServers.Count == 0)
        {
            logger.LogError("No servers with subregion {SubRegion} found", subRegion);
            return;
        }
        await PreparePuppeteer(cancellationToken);
        var progress = context.WriteProgressBar();
        foreach (var server in subRegionServers.WithProgress(progress))
        {
            await FindMinGuildIdServer(server, cancellationToken);
        }
        logger.LogInformation("Finished scrapping guild id for {SubRegion}", subRegion);
    }

    public async Task ExecuteAsync(PerformContext context, int serverId, CancellationToken cancellationToken = default)
    {
        var server = await lomDbContext.Servers.FirstOrDefaultAsync(x => x.ServerId == serverId, cancellationToken: cancellationToken);
        if (server is null)
        {
            logger.LogError("Server with id {ServerId} not found", serverId);
            return;
        }
        await PreparePuppeteer(cancellationToken);
        await FindMinGuildIdServer(server, cancellationToken);
    }


    private async Task FindMinGuildIdServer(Server server, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting find min guild id job for server {ServerId}", server.ServerId);
        var previousServer = await lomDbContext.Servers.FirstOrDefaultAsync(x => x.ServerId == server.ServerId - 1, cancellationToken: cancellationToken);
        if (previousServer is not null && server.MinGuildId is not null)
        {
            logger.LogInformation("Server {ServerId} already has min guild id", server.ServerId);
            return;
        }
        if(previousServer is null && server.MinGuildId is null)
        {
            logger.LogError("Server {ServerId} has no min guild id", server.ServerId);
            return;
        }
        _minGuildId = previousServer is null ? server.MinGuildId!.Value : ExtrapolateBeginGuildId(previousServer.MinGuildId!.Value);
        var maxGuildId = _minGuildId + 20000;
        while (!_guildFound && _minGuildId < maxGuildId)
        {
            await browserService.WriteToConsole($"netManager.send(\"guild.guild_members_info_c2s\", {{ guild_id: {_minGuildId}, source: undefined }}, false);");
            _minGuildId += 10;
            await Task.Delay(100, cancellationToken);
        }
        server.MinGuildId = _minGuildId - 50;
        await lomDbContext.SaveChangesAsync(cancellationToken);
        _minGuildId = 0;
        _guildFound = false;
        logger.LogInformation("Finished scrapping guild id for server {ServerId}", server.ServerId);
    }

    private async Task PreparePuppeteer(CancellationToken cancellationToken)
    {
        await Task.Delay(8000, cancellationToken);
        await browserService.ChangePrintLevel();
        //Hook to page console
        browserService.ConsoleMessageEvent += async (sender, e) => await ConsoleMessageReceived(sender, e);
    }

    private static ulong ExtrapolateBeginGuildId(ulong previousServerMinGuildId)
    {
        var guildId = previousServerMinGuildId + 134210000;
        return guildId - guildId % 10000;
    }
    
    private async Task ConsoleMessageReceived(object? sender, ConsoleMessageEvent e)
    {
        switch (e.Message)
        {
            case "guild.guild_members_info_s2c":
                var jsonValue = await e.Response!.JsonValueAsync();
                var stringValue = jsonValue.ToString();
                var guildMemberInfos = JsonSerializer.Deserialize<GuildMembersInfoResponse>(stringValue!);
                if (guildMemberInfos is not null && guildMemberInfos.MemberList.Count > 0)
                {
                    logger.LogInformation("Members count: {MembersCount}", guildMemberInfos!.MemberList.Count);
                    logger.LogInformation("guild_id: {GuildId}", guildMemberInfos.GuildId);
                    _guildFound = true;
                    _minGuildId = guildMemberInfos.GuildId;
                }
                break;
        }
    }
}