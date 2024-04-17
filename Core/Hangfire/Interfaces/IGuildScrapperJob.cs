using Entities.Models;
using Hangfire;
using Hangfire.Server;

namespace Core.Hangfire.Interfaces;

#if DEBUG
[Queue(WorkerConstants.Queues.Dev)]
#else
[Queue(WorkerConstants.Queues.Parsing)]
#endif
public interface IGuildScrapperJob
{
    [JobDisplayName("GuildInfoScrapperJob : {1}")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ExecuteAsync(PerformContext context, SubRegion subRegion, CancellationToken cancellationToken = default);

    [JobDisplayName("GuildInfoScrapperJob : {1}")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ExecuteAsync(PerformContext context, int serverId, CancellationToken cancellationToken = default);
}