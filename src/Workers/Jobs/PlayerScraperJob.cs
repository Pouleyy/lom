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
using System.Collections.Concurrent;
using System.Text.Json;
using Server = Entities.Models.Server;

namespace Jobs.Jobs;

public class PlayerScraperJob(LomDbContext lomDbContext, IBrowserService browserService, ILogger<PlayerScraperJob> logger) : IPlayerScraperJob
{
    private ConcurrentDictionary<ulong, Player> _currentPlayers = [];
    private int _currentServerId;
    private BrowserLom? _browser;

    public async Task ExecuteAsync(PerformContext context, SubRegion subRegion, bool top3 = false, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting player scrapper job for {SubRegion}", subRegion);
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
                await ScrapPlayerInfo(server, top3, cancellationToken);
            }
        }
        finally
        {
            if (_browser is not null)
            {
                await _browser.CloseBrowser();
                browserService.ReleaseBrowser(_browser);
                _browser.ConsoleMessageEvent -= async (sender, e) => await ConsoleMessageReceived(sender, e);
            }
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
        try
        {
            await PrepareBrowser(server.SubRegion, cancellationToken);
            await ScrapPlayerInfo(server, false, cancellationToken);
        }
        finally
        {
            if (_browser is not null)
            {
                await _browser.CloseBrowser();
                browserService.ReleaseBrowser(_browser);
                _browser.ConsoleMessageEvent -= async (sender, e) => await ConsoleMessageReceived(sender, e);
            }
        }
    }

    private async Task ScrapPlayerInfo(Server server, bool top3, CancellationToken cancellationToken)
    {
        logger.LogTrace("Starting scrapping player id for server {ServerId}", server.ServerId);
        //await _browser!.ChangePageTitle($"{nameof(PlayerScraperJob)} - {server.ServerId} - {(top3 ? "Top 3" : "Full")}");
        var guildIds = await GetGuildsIdToScrap(server, top3, cancellationToken);
        var serverPlayers = await lomDbContext.Players.Where(x => x.ServerId == server.ServerId).ToDictionaryAsync(x => x.PlayerId, cancellationToken: cancellationToken);
        _currentPlayers = new ConcurrentDictionary<ulong, Player>(serverPlayers);
        _currentServerId = server.ServerId;
        foreach (var guildId in guildIds)
        {
            await _browser.WriteToConsole($"netManager.send(\"guild.guild_members_info_c2s\", {{ guild_id: {guildId}, source: undefined }}, false);");
            await Task.Delay(100, cancellationToken);
        }
        await Task.Delay(2000, cancellationToken);
        logger.LogDebug("{CurrentPlayersCount} players found for server {ServerId}", _currentPlayers.Count, server.ServerId);
        var playerToProcess = top3 ? _currentPlayers.Where(x => x.Value.GuildId != null && guildIds.Contains(x.Value.GuildId.Value)) : _currentPlayers;
        foreach (var (playerId, _) in playerToProcess)
        {
            await _browser.WriteToConsole($"netManager.send(\"role.role_others_c2s\", {{ role_id: [{playerId}], source: undefined }}, false);");
            await Task.Delay(100, cancellationToken);
        }
        await Task.Delay(2000, cancellationToken);
        var newPlayers = _currentPlayers.Except(serverPlayers).Select(x => x.Value).ToList();
        logger.LogDebug("{NewPlayersCount} new players to add", newPlayers.Count);
        logger.LogDebug("New players ids : {PlayerIds}", string.Join(", ", newPlayers.Select(x => x.PlayerId)));
        await lomDbContext.Players.AddRangeAsync(newPlayers, cancellationToken);
        await lomDbContext.SaveChangesAsync(cancellationToken);
        _currentPlayers.Clear();
        logger.LogInformation("{ServerId} - player info scraped", server.ServerId);
    }

    private async Task<List<ulong>> GetGuildsIdToScrap(Server server, bool top3, CancellationToken cancellationToken)
    {
        return top3
            ? await lomDbContext.Families
                                .Where(x => x.ServerId == server.ServerId)
                                .Select(family => new
                                 {
                                     Family = family,
                                     TotalPower = family.Players.Sum(player => (long)player.Power)
                                 })
                                .OrderByDescending(x => x.TotalPower)
                                .Take(3)
                                .Select(x => x.Family.GuildId)
                                .ToListAsync(cancellationToken)
            : await lomDbContext.Families.Where(x => x.ServerId == server.ServerId).Select(x => x.GuildId).ToListAsync(cancellationToken);
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
        await _browser.Initialize();
        _browser.ConsoleMessageEvent += async (sender, e) => await ConsoleMessageReceived(sender, e);
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
                        player.GuildId = guildMemberInfos.GuildId;
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
                            ServerId = _currentServerId,
                            DonationWeekly = member.DonateWeek,
                            ProfilePictureUrl = member.RoleHead.Url,
                            Role = Enum.Parse<Role>(member.Role.ToString()),
                            LastLogin = member.IsOnline == 1 ? DateTime.UtcNow : DateTimeOffset.FromUnixTimeSeconds(member.LastLogin).UtcDateTime,
                            LastUpdate = DateTime.UtcNow
                        };
                        _currentPlayers.TryAdd(member.PlayerId, newPlayer);
                    }
                }
                //For each player with guildId in guildMemberInfos.GuildId but without in guildMemberInfos.MemberList, set guildId to 0 because not in guild anymore
                foreach (var (playerId, player) in _currentPlayers.Where(x => x.Value.GuildId == guildMemberInfos.GuildId))
                {
                    if (guildMemberInfos.MemberList.Any(x => x.PlayerId == playerId)) continue;
                    player.GuildId = null;
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
                    playerToUpdate.Level = (int)playerInfos.InfoList.KeyValues.FirstOrDefault(x => x.Key == 1001)!.Value;
                    playerToUpdate.Attack = playerInfos.SpList.FirstOrDefault(x => x.Key == 1)!.Value;
                    playerToUpdate.Defense = playerInfos.SpList.FirstOrDefault(x => x.Key == 24)!.Value;
                    playerToUpdate.Health = playerInfos.SpList.FirstOrDefault(x => x.Key == 2)!.Value;
                    playerToUpdate.CritRate = (int)playerInfos.SpList.FirstOrDefault(x => x.Key == 1004)!.Value;
                    playerToUpdate.CritMultiplier = (int)playerInfos.SpList.FirstOrDefault(x => x.Key == 1005)!.Value;
                    playerToUpdate.CritRes = (int)playerInfos.SpList.FirstOrDefault(x => x.Key == 1006)!.Value;
                    playerToUpdate.Evasion = (int)playerInfos.SpList.FirstOrDefault(x => x.Key == 1008)!.Value;
                    playerToUpdate.Combo = (int)playerInfos.SpList.FirstOrDefault(x => x.Key == 1016)!.Value;
                    playerToUpdate.Counterstrike = (int)playerInfos.SpList.FirstOrDefault(x => x.Key == 1017)!.Value;
                    playerToUpdate.Stun = (int)playerInfos.SpList.FirstOrDefault(x => x.Key == 1023)!.Value;
                    playerToUpdate.ComboMultiplier = (int)playerInfos.SpList.FirstOrDefault(x => x.Key == 1032)!.Value;
                    playerToUpdate.CounterstrikeMultiplier = (int)playerInfos.SpList.FirstOrDefault(x => x.Key == 1033)!.Value;
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