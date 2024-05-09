using Core.Hangfire.Interfaces;
using Core.Services.Interface;
using Core.Services.Models;
using Entities.Context;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jobs.Jobs;

public class PlayerLeaderboardGSheetJob(LomDbContext dbContext, IGSheetService gSheetService, ILogger<PlayerLeaderboardGSheetJob> logger) : IPlayerLeaderboardGSheetJob
{
    public async Task ExecuteAsync(PerformContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting Player Leaderboard GSheet job");
        var topPlayers = await dbContext.Players
            .Select(x => new PlayerLeaderboard
            {
                Name = x.PlayerName,
                ServerId = x.ServerId ?? 0,
                Power = (long)x.Power,
                SubRegion = x.Server!.SubRegion
            })
            .GroupBy(x => x.ServerId)
            .Select(x => x.OrderByDescending(y => y.Power).Take(300))
            .ToListAsync(cancellationToken);
        var players = topPlayers.SelectMany(x => x).ToList();
        var topPlayerPerSubRegion = players.GroupBy(x => x.SubRegion);
        await gSheetService.WriteTopPlayers(topPlayerPerSubRegion, cancellationToken);
        logger.LogInformation("Finished Player Leaderboard GSheet job");
    }
}