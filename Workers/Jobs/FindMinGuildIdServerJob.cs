using Core.Services;
using Core.Services.Models;
using Entities.Context;
using Hangfire;
using Hangfire.Server;
using Lom.Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;
using System.Text.Json;

namespace Jobs.Jobs;

#if DEBUG
[Queue(WorkerConstants.Queues.Dev)]
#else
[Queue(WorkerConstants.Queues.Parsing)]
#endif
public class FindMinGuildIdServerJob(LomDbContext lomDbContext, BrowserService browserService, ILogger<FindMinGuildIdServerJob> logger)
{
    private bool _guildFound = false;
    private long _minGuildId;

    [JobDisplayName("FindMinGuildIdServerJob : {1}")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public async Task ExecuteAsync(PerformContext context, int serverId, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting find min guild id job");
        var server = await lomDbContext.Servers.FirstOrDefaultAsync(x => x.ServerId == serverId, cancellationToken: cancellationToken);
        var previousServer = await lomDbContext.Servers.FirstOrDefaultAsync(x => x.ServerId == serverId - 1, cancellationToken: cancellationToken);
        if (server is null)
        {
            logger.LogError("Server with id {ServerId} not found", serverId);
            return;
        }
        _minGuildId = previousServer is null ? server.MinGuildId!.Value : ExtrapolateBeginGuildId(previousServer.MinGuildId!.Value);
        //Wait for the browser to be ready
        await Task.Delay(8000, cancellationToken);
        await browserService.ChangePrintLevel();
        //Hook to page console
        browserService.ConsoleMessageEvent += async (sender, e) => await ConsoleMessageReceived(sender, e);
        var maxGuildId = _minGuildId + 20000; 
        while (!_guildFound && _minGuildId < maxGuildId)
        {
            await browserService.WriteToConsole($"netManager.send(\"guild.guild_members_info_c2s\", {{ guild_id: {_minGuildId}, source: undefined }}, false);");
            _minGuildId += 10;
            await Task.Delay(100, cancellationToken);
        }
        server!.MinGuildId = _minGuildId - 100;
        await lomDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Finished Lom scrapper job");
    }

    private static long ExtrapolateBeginGuildId(long previousServerMinGuildId)
    {
        var guildId = previousServerMinGuildId + 134210000;
        return guildId - guildId % 10000;
    }


    private async Task ConsoleMessageReceived(object? sender, ConsoleMessageEvent e)
    {
        var message = e.Message.Split(' ');
        if (message.Length < 3) return;
        switch (message[2])
        {
            case "guild.guild_info_s2c":
                logger.LogInformation("Guild info received");
                break;
            case "guild.guild_members_info_s2c":
                logger.LogInformation("Guild members info received");
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