using Entities.Models;
using Hangfire;
using Hangfire.Server;

namespace Core.Hangfire.Interfaces;

#if DEBUG
[Queue(WorkerConstants.Queues.Dev)]
#else
[Queue(WorkerConstants.Queues.Scraping)]
#endif
public interface IGuildScraperJob
{
    [JobDisplayName("Guild info : {1}")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ExecuteAsync(PerformContext context, SubRegion subRegion, CancellationToken cancellationToken = default);

    [JobDisplayName("Guild info : {1}")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ExecuteAsync(PerformContext context, int serverId, CancellationToken cancellationToken = default);
}