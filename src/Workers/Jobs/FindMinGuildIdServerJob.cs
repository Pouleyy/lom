using Core.Hangfire.Interfaces;
using Core.Helpers;
using Core.Services.Interface;
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

public class FindMinGuildIdServerJob(LomDbContext lomDbContext, IBrowserService browserService, ILogger<FindMinGuildIdServerJob> logger) : IFindMinGuildIdServerJob
{
    private bool _guildFound;
    private ulong _minGuildId;
    private BrowserLom? _browser;

    public async Task ExecuteAsync(PerformContext context, SubRegion subRegion, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting find min guild id job for {SubRegion}", subRegion);
        var subRegionServers = await lomDbContext.Servers.OrderBy(x => x.ServerId).Where(x => x.SubRegion == subRegion).ToListAsync(cancellationToken: cancellationToken);
        if (subRegionServers.Count == 0)
        {
            logger.LogError("No servers with subregion {SubRegion} found", subRegion);
            return;
        }
        try
        {
            await PrepareBrowser(subRegion, cancellationToken);
            var progress = context.WriteProgressBar();
            foreach (var server in subRegionServers.WithProgress(progress))
            {
                await FindMinGuildIdServer(server, cancellationToken);
            }
        }
        finally
        {
            if (_browser is not null)
            {
                browserService.ReleaseBrowser(_browser);
                _browser.ConsoleMessageEvent -= async (sender, e) => await ConsoleMessageReceived(sender, e);
            }
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
        try
        {
            await PrepareBrowser(server.SubRegion, cancellationToken);
            await FindMinGuildIdServer(server, cancellationToken);
        }
        finally
        {
            if (_browser is not null)
            {
                browserService.ReleaseBrowser(_browser);
                _browser.ConsoleMessageEvent -= async (sender, e) => await ConsoleMessageReceived(sender, e);
            }
        }
    }


    private async Task FindMinGuildIdServer(Server server, CancellationToken cancellationToken)
    {
        logger.LogDebug("Starting find min guild id job for server {ServerId}", server.ServerId);
        //await _browser!.ChangePageTitle($"{nameof(FindMinGuildIdServerJob)} - {server.ServerId}");
        var previousServer = await lomDbContext.Servers.FirstOrDefaultAsync(x => x.ServerId == server.ServerId - 1, cancellationToken: cancellationToken);
        if (previousServer is not null && server.MinGuildId is not null)
        {
            logger.LogInformation("Server {ServerId} already has min guild id", server.ServerId);
            return;
        }
        if (previousServer is null)
        {
            if (server.MinGuildId is not null && server.MinGuildId % 10000 != 0000)
            {
                logger.LogInformation("First server {ServerId} for sub region {SubRegion} already process", server.ServerId, server.SubRegion);
                return;
            }
            if (server.MinGuildId is null)
            {
                logger.LogCritical("Server {ServerId} for sub region {SubRegion} has no min guild id", server.ServerId, server.SubRegion);
                return;
            }
        }
        _minGuildId = previousServer is null ? server.MinGuildId!.Value : ExtrapolateBeginGuildId(previousServer.MinGuildId!.Value);
        var maxGuildId = _minGuildId + 20000;
        while (!_guildFound && _minGuildId < maxGuildId)
        {
            await _browser.WriteToConsole($"netManager.send(\"guild.guild_info_c2s\", {{ guild_id: {_minGuildId} }}, false);");
            _minGuildId += 10;
            await Task.Delay(100, cancellationToken);
        }
        server.MinGuildId = _minGuildId - 50;
        await Task.Delay(500, cancellationToken);
        await lomDbContext.SaveChangesAsync(cancellationToken);
        _minGuildId = 0;
        _guildFound = false;
        logger.LogInformation("{ServerId} - min guild id scraped", server.ServerId);
        await Task.Delay(2000, cancellationToken);
    }

    private async Task PrepareBrowser(SubRegion subRegion, CancellationToken cancellationToken)
    {
        var browser = await browserService.GetBrowser(RegionHelper.SubRegionToRegion(subRegion), cancellationToken);
        if (browser is null)
        {
            logger.LogError("No browser found for {SubRegion}", subRegion);
            return;
        }
        _browser = browser;
        _browser.ConsoleMessageEvent += async (sender, e) => await ConsoleMessageReceived(sender, e);
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
            case "guild.guild_info_s2c":
                var jsonValue = await e.Response!.JsonValueAsync();
                var stringValue = jsonValue.ToString();
                var guildInfo = JsonSerializer.Deserialize<GuildInfoResponse>(stringValue!);
                _guildFound = true;
                _minGuildId = guildInfo!.GuildInfo.GuildId;
                logger.LogTrace("Min guild found {GuildId}", _minGuildId);
                break;
        }
    }
}