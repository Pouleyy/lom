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

public class GuildScraperJob(LomDbContext lomDbContext, IBrowserService browserService, ILogger<GuildScraperJob> logger) : IGuildScraperJob
{
    private int _numberOfVoidIds;
    private const int _limitVoidIds = 75;
    private int _currentServerId;
    private List<ulong> _currentGuildIds = [];
    private BrowserLom _browser;
    
    public async Task ExecuteAsync(PerformContext context, SubRegion subRegion, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting guild scrapper job for {SubRegion}", subRegion);
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
                await ScrapGuildInfo(server, cancellationToken);
            }
        }
        finally
        {
            browserService.ReleaseBrowser(_browser);
        }
        logger.LogInformation("Finished scrapping guild id for {SubRegion}", subRegion);
    }
    
    public async Task ExecuteAsync(PerformContext context, int serverId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting guild scrapper job for server {ServerId}", serverId);
        var server = await lomDbContext.Servers.FirstOrDefaultAsync(x => x.ServerId == serverId, cancellationToken: cancellationToken);
        if (server is null)
        {
            logger.LogError("Server with id {ServerId} not found", serverId);
            return;
        }
        try
        {
            await PrepareBrowser(server.SubRegion, cancellationToken);
            await ScrapGuildInfo(server, cancellationToken);
        }
        finally
        {
            browserService.ReleaseBrowser(_browser);
        }
    }

    private async Task ScrapGuildInfo(Server server, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting scrapping guild id for {ServerId}", server.ServerId);
        if (server.MinGuildId is null)
        {
            logger.LogError("Server {ServerId} has no min guild id", server.ServerId);
            return;
        }
        _currentServerId = server.ServerId;
        _currentGuildIds = await lomDbContext.Families.Where(x => x.ServerId == server.ServerId).Select(x => x.GuildId).ToListAsync(cancellationToken);
        var minGuildId = server.MinGuildId;
        while (_numberOfVoidIds < _limitVoidIds)
        {
            await _browser.WriteToConsole($"netManager.send(\"guild.guild_info_c2s\", {{ guild_id: {minGuildId}, source: undefined }}, false);");
            minGuildId++;
            _numberOfVoidIds++;
            await Task.Delay(100, cancellationToken);
        }
        _numberOfVoidIds = 0;
        server.MinGuildId = _currentGuildIds.Min();
        await lomDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task PrepareBrowser(SubRegion subRegion, CancellationToken cancellationToken)
    {
        var browser = await browserService.GetBrowser(RegionHelper.SubRegionToRegion(subRegion));
        if (browser is null)
        {
            logger.LogError("No browser found for {SubRegion}", subRegion);
            return;
        }
        _browser = browser;
        _browser.ConsoleMessageEvent += async (sender, e) => await ConsoleMessageReceived(sender, e);
    }

    private async Task ConsoleMessageReceived(object? sender, ConsoleMessageEvent e)
    {
        switch (e.Message)
        {
            case "guild.guild_info_s2c":
                var jsonValue = await e.Response!.JsonValueAsync();
                var stringValue = jsonValue.ToString();
                var guildInfoResponse = JsonSerializer.Deserialize<GuildInfoResponse>(stringValue!);
                var guildInfo = guildInfoResponse!.GuildInfo;
                logger.LogInformation("Guild members info received for guild {GuildId}", guildInfo.GuildId);
                if (!_currentGuildIds.Contains(guildInfo.GuildId))
                {
                    var guild = new Family
                    {
                        GuildId = guildInfo.GuildId,
                        GuildName = guildInfo.GuildName,
                        Level = guildInfo.Level,
                        Notice = guildInfo.Notice,
                        CreatedTime = DateTimeOffset.FromUnixTimeSeconds(guildInfo.CreateTime).UtcDateTime,
                        LeaderId = guildInfo.LeaderId,
                        ServerId = _currentServerId
                    };
                    await lomDbContext.Families.AddAsync(guild);
                }
                _numberOfVoidIds = 0;
                break;
        }
    }
}