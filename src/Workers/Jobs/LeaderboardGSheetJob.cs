using Core.Hangfire.Interfaces;
using Core.Services.Interface;
using Core.Services.Models;
using Entities.Context;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jobs.Jobs;

public class LeaderboardGSheetJob(LomDbContext lomDbContext, IGSheetService gSheetService, ILogger<LeaderboardGSheetJob> logger) : ILeaderboardGSheetJob
{
    public async Task ExecuteAsync(PerformContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting Leaderboard GSheet job");
        //Get top 10 guild by player power for each server
        var top10FamiliesPerServer = await lomDbContext.Families.Select(x => new FamilyLeadboard
                                                        {
                                                          FamilyName = x.GuildName,
                                                          ServerId = x.ServerId,
                                                          Power = x.Players.Sum(player => (long)player.Power)
                                                      })
                                                     .GroupBy(x => x.ServerId)
                                                     .Select(x => x.OrderByDescending(y => y.Power).Take(10))
                                                     .ToListAsync(cancellationToken);
        await gSheetService.WriteTop10Guilds(top10FamiliesPerServer, cancellationToken);
        logger.LogInformation("Finished Leaderboard GSheet job");
    }
}