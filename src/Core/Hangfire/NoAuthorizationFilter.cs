using Hangfire.Dashboard;

namespace Core.Hangfire;

public class NoAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}