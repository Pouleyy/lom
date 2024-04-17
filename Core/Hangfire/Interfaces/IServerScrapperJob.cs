using Hangfire;
using Hangfire.Server;

namespace Core.Hangfire.Interfaces;

#if DEBUG
[Queue(WorkerConstants.Queues.Dev)]
#else
[Queue(WorkerConstants.Queues.ServerParsing)]
#endif
public interface IServerScrapperJob
{
    [JobDisplayName("Server Parse Job")]
    Task ExecuteAsync(PerformContext context, CancellationToken cancellationToken = default);

}