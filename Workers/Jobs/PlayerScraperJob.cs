using Core.Hangfire;
using Core.Hangfire.Interfaces;
using Core.Services;
using Core.Services.Models;
using Entities.Context;
using Entities.Models;
using Hangfire;
using Hangfire.Console;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Text.Json;
using Server = Entities.Models.Server;

namespace Jobs.Jobs;

public class PlayerScraperJob(LomDbContext lomDbContext, BrowserService browserService, ILogger<PlayerScraperJob> logger) : IPlayerScraperJob
{
    private ConcurrentDictionary<ulong, Player> _currentPlayers = [];

    public async Task ExecuteAsync(PerformContext context, SubRegion subRegion, bool top10 = false, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting player scrapper job for {SubRegion}", subRegion);
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
            await ScrapPlayerInfo(server, top10, cancellationToken);
        }
        logger.LogInformation("Finished scrapping player id for {SubRegion}", subRegion);
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
        await ScrapPlayerInfo(server, false, cancellationToken);
    }

    private async Task ScrapPlayerInfo(Server server, bool top10, CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting scrapping player id for server {ServerId}", server.ServerId);
        var guildIds = await GetGuildsIdToScrap(server, top10, cancellationToken);
        var serverPlayers = await lomDbContext.Players.Where(x => guildIds.Contains(x.GuildId)).ToDictionaryAsync(x => x.PlayerId, cancellationToken: cancellationToken);
        _currentPlayers = new ConcurrentDictionary<ulong, Player>(serverPlayers);
        foreach (var guildId in guildIds)
        {
            await browserService.WriteToConsole($"netManager.send(\"guild.guild_members_info_c2s\", {{ guild_id: {guildId}, source: undefined }}, false);");
            await Task.Delay(100, cancellationToken);
        }
        await Task.Delay(2000, cancellationToken);
        foreach (var (playerId, _) in _currentPlayers)
        {
            await browserService.WriteToConsole($"netManager.send(\"role.role_others_c2s\", {{ role_id: [{playerId}], source: undefined }}, false);");
            await Task.Delay(100, cancellationToken);
        }
        await Task.Delay(2000, cancellationToken);
        await lomDbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Finished scrapping player id for server {ServerId}", server.ServerId);
    }

    private async Task<List<ulong>> GetGuildsIdToScrap(Server server, bool top10, CancellationToken cancellationToken)
    {
        return top10
            ? await lomDbContext.Families
                                .Where(x => x.ServerId == server.ServerId)
                                .Select(family => new
                                 {
                                     Family = family,
                                     TotalPower = family.Players.Sum(player => (long)player.Power)
                                 })
                                .OrderByDescending(x => x.TotalPower)
                                .Take(10)
                                .Select(x => x.Family.GuildId)
                                .ToListAsync(cancellationToken)
            : await lomDbContext.Families.Where(x => x.ServerId == server.ServerId).Select(x => x.GuildId).ToListAsync(cancellationToken);
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
        switch (e.Message)
        {
            case "guild.guild_members_info_s2c":
                var jsonValue = await e.Response!.JsonValueAsync();
                var stringValue = jsonValue.ToString();
                var guildMemberInfos = JsonSerializer.Deserialize<GuildMembersInfoResponse>(stringValue!);
                if (guildMemberInfos is null || guildMemberInfos.MemberList.Count <= 0)
                {
                    break;
                }
                foreach (var member in guildMemberInfos.MemberList)
                {
                    if (_currentPlayers.TryGetValue(member.PlayerId, out var player))
                    {
                        player.PlayerName = member.PlayerName;
                        player.Role = Enum.Parse<Role>(member.Role.ToString());
                        player.LastLogin = member.IsOnline == 1 ? DateTime.UtcNow : DateTimeOffset.FromUnixTimeSeconds(member.LastLogin).UtcDateTime;
                        player.DonationWeekly = member.DonateWeek;
                        player.ProfilePictureUrl = member.RoleHead.Url;
                        player.LastUpdate = DateTime.UtcNow;
                    }
                    else
                    {
                        var newPlayer = new Player
                        {
                            PlayerId = member.PlayerId,
                            Uid = ConvertPlayerIdToUid(member.PlayerId),
                            PlayerName = member.PlayerName,
                            GuildId = guildMemberInfos.GuildId,
                            Role = Enum.Parse<Role>(member.Role.ToString()),
                            LastLogin = member.IsOnline == 1 ? DateTime.UtcNow : DateTimeOffset.FromUnixTimeSeconds(member.LastLogin).UtcDateTime,
                            LastUpdate = DateTime.UtcNow
                        };
                        _currentPlayers.AddOrUpdate(member.PlayerId, newPlayer, (key, oldValue) => newPlayer);
                        await lomDbContext.Players.AddAsync(newPlayer);
                    }
                }
                break;
            case "role.role_others_s2c":
                var jsonMember = await e.Response!.JsonValueAsync();
                var stringMember = jsonMember.ToString();
                var roleOthersResponse = JsonSerializer.Deserialize<RoleOthersResponse>(stringMember!);
                if (roleOthersResponse is null || roleOthersResponse.Players.Length <= 0) return;
                var playerInfos = roleOthersResponse.Players.First();
                if (_currentPlayers.TryGetValue(playerInfos.PlayerId, out var playerToUpdate))
                {
                    playerToUpdate.Power = playerInfos.InfoList.KeyValues.FirstOrDefault(x => x.Key == 1020)!.Value;
                    playerToUpdate.Attack = playerInfos.SpList.FirstOrDefault(x => x.Key == 1)!.Value;
                    playerToUpdate.Defense = playerInfos.SpList.FirstOrDefault(x => x.Key == 24)!.Value;
                    playerToUpdate.Health = playerInfos.SpList.FirstOrDefault(x => x.Key == 2)!.Value;
                }
                break;
        }
    }

    private string ConvertPlayerIdToUid(ulong playerId)
    {
        var uid = (playerId & 134217727).ToString("X");
        while (uid.Length < 5)
        {
            uid = "0" + uid;
        }
        return $"{uid[2]}{uid[1]}{uid[3]}{uid[0]}{uid[4]}".ToUpper();
    }
}