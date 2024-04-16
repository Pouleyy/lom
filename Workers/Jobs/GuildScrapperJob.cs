using Core.Services;
using Core.Services.Models;
using Entities.Context;
using Entities.Models;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Lom.Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Server = Entities.Models.Server;

namespace Jobs.Jobs;

#if DEBUG
[Queue(WorkerConstants.Queues.Dev)]
#else
[Queue(WorkerConstants.Queues.Parsing)]
#endif
public class GuildScrapperJob(LomDbContext lomDbContext, BrowserService browserService, ILogger<GuildScrapperJob> logger)
{
    private int _numberOfVoidIds;
    private const int _limitVoidIds = 75;
    private int _currentServerId;
    private List<ulong> _currentGuildIds = [];
    
    [JobDisplayName("GuildInfoScrapperJob : {1}")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ExecuteAsync(PerformContext context, ServerShortName subRegion, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting guild scrapper job for {SubRegion}", subRegion);
        var subRegionServers = await lomDbContext.Servers.OrderBy(x => x.ServerId).Where(x => x.ShortName == subRegion).ToListAsync(cancellationToken: cancellationToken);
        if (subRegionServers.Count == 0)
        {
            logger.LogError("No servers with subregion {SubRegion} found", subRegion);
            return;
        }
        await PreparePuppeteer(cancellationToken);
        var progress = context.WriteProgressBar();
        foreach (var server in subRegionServers.WithProgress(progress))
        {
            await ScrapGuildInfo(server, cancellationToken);
        }
        logger.LogInformation("Finished scrapping guild id for {SubRegion}", subRegion);
    }
    
    [JobDisplayName("GuildInfoScrapperJob : {1}")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ExecuteAsync(PerformContext context, int serverId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting guild scrapper job for server {ServerId}", serverId);
        var server = await lomDbContext.Servers.FirstOrDefaultAsync(x => x.ServerId == serverId, cancellationToken: cancellationToken);
        if (server is null)
        {
            logger.LogError("Server with id {ServerId} not found", serverId);
            return;
        }
        await PreparePuppeteer(cancellationToken);
        await ScrapGuildInfo(server, cancellationToken);
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
            await browserService.WriteToConsole($"netManager.send(\"guild.guild_info_c2s\", {{ guild_id: {minGuildId}, source: undefined }}, false);");
            minGuildId++;
            _numberOfVoidIds++;
            await Task.Delay(100, cancellationToken);
        }
        _numberOfVoidIds = 0;
        await lomDbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task PreparePuppeteer(CancellationToken cancellationToken)
    {
        await Task.Delay(8000, cancellationToken);
        await browserService.ChangePrintLevel();
        //Hook to page console
        browserService.ConsoleMessageEvent += async (sender, e) => await ConsoleMessageReceived(sender, e);
    }

    private async Task ConsoleMessageReceived(object? sender, ConsoleMessageEvent e)
    {
        var message = e.Message.Split(' ');
        if (message.Length < 3) return;
        switch (message[2])
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
                        CreatedTime = DateTimeOffset.FromUnixTimeSeconds((long)guildInfo.CreateTime).UtcDateTime,
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