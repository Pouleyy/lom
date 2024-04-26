using Core.Hangfire.Interfaces;
using Core.Services.Interface;
using Core.Services.Models;
using Entities.Context;
using Entities.Models;
using Hangfire;
using Hangfire.Server;
using Hangfire.Storage;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Jobs.Jobs;

public class LeaderboardGSheetJob(LomDbContext lomDbContext, IGSheetService gSheetService, ILogger<LeaderboardGSheetJob> logger) : ILeaderboardGSheetJob
{
    public async Task ExecuteAsync(PerformContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting Leaderboard GSheet job");
        //Get top 10 guild by player power for each server
        var top10FamiliesPerServer = await lomDbContext.Families.Where(x => x.ServerId != 0).Select(x => new FamilyLeadboard
                                                        {
                                                            FamilyName = x.GuildName,
                                                            ServerId = x.ServerId,
                                                            Power = x.Players.Sum(player => (long)player.Power)
                                                        })
                                                       .GroupBy(x => x.ServerId)
                                                       .Select(x => x.OrderByDescending(y => y.Power).Take(10))
                                                       .ToListAsync(cancellationToken);
        top10FamiliesPerServer = top10FamiliesPerServer.Where(x => x.Count() == 10).ToList();
        await gSheetService.WriteTop10Guilds(top10FamiliesPerServer, cancellationToken);
        var lastExecutionTimeBySubRegion = GetLastExecutionTimeBySubRegion();
        await gSheetService.WriteLastExecutionTimeBySubRegion(lastExecutionTimeBySubRegion, cancellationToken);
        logger.LogInformation("Finished Leaderboard GSheet job");
    }

    private static Dictionary<SubRegion, (long full, long top3)> GetLastExecutionTimeBySubRegion()
    {
        var lastExecutionTimeBySubRegion = new Dictionary<SubRegion, (long full, long top3)>();
        using var connection = JobStorage.Current.GetConnection();
        foreach (var subregion in Enum.GetValues<SubRegion>())
        {
            var recurringJobFull = connection.GetRecurringJobs().FirstOrDefault(x => x.Id == $"Player info full - {subregion}");
            var recurringJobTop3 = connection.GetRecurringJobs().FirstOrDefault(x => x.Id == $"Player info top 3 - {subregion}");
            var fullTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var top3Timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (recurringJobFull != null && recurringJobFull.LastExecution.HasValue)
            {
                fullTimestamp = ((DateTimeOffset)recurringJobFull.LastExecution.Value).ToUnixTimeMilliseconds();
            }
            if (recurringJobTop3 != null && recurringJobTop3.LastExecution.HasValue)
            {
                top3Timestamp = ((DateTimeOffset)recurringJobTop3.LastExecution.Value).ToUnixTimeMilliseconds();
            }
            lastExecutionTimeBySubRegion.Add(subregion, (fullTimestamp, top3Timestamp));
        }
        return lastExecutionTimeBySubRegion;
    }
}