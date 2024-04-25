using Entities.Models;
using Hangfire;
using Hangfire.Server;

namespace Core.Hangfire.Interfaces;

#if DEBUG
[Queue(WorkerConstants.Queues.Dev)]
#else
[Queue(WorkerConstants.Queues.Scraping)]
#endif
public interface IPlayerScraperJob
{
    [JobDisplayName("Player info : {1} - {2}")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ExecuteAsync(PerformContext context, SubRegion subRegion, bool top3 = false, CancellationToken cancellationToken = default);

    [JobDisplayName("Player info : {1}")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ExecuteAsync(PerformContext context, int serverId, CancellationToken cancellationToken = default);

}