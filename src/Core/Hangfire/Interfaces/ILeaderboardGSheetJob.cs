using Hangfire;
using Hangfire.Server;

namespace Core.Hangfire.Interfaces;

#if DEBUG
[Queue(WorkerConstants.Queues.Dev)]
#else
[Queue(WorkerConstants.Queues.GSheet)]
#endif
public interface ILeaderboardGSheetJob
{
    [JobDisplayName("Leaderboard GSheet")]
    [AutomaticRetry(Attempts = WorkerConstants.TotalRetry, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    Task ExecuteAsync(PerformContext context, CancellationToken cancellationToken = default);
}