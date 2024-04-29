using Core.Hangfire;
using Core.Hangfire.Interfaces;
using Core.Helpers;
using Entities.Models;
using Hangfire;
using Hangfire.Console.Extensions;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Hangfire.Console;

namespace Core.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration) =>
        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(configuration.GetConnectionString("Hangfire") ??
                                                       throw new ArgumentNullException("Hangfire connection string not specified in configuration")),
                new PostgreSqlStorageOptions { InvisibilityTimeout = TimeSpan.FromHours(10) }).WithJobExpirationTimeout(TimeSpan.FromDays(30)).UseConsole());

    public static void AddHangfireConsoleExtension(this IServiceCollection services) => services.AddHangfireConsoleExtensions();

    public static void AddHangfireServer(this IServiceCollection services, IConfiguration configuration) =>
        services.AddHangfireServer(o =>
        {
            o.WorkerCount = configuration.GetValue<int>("Hangfire:WorkerCount");
            o.Queues = configuration.GetValue<string>("Hangfire:Queues")?.Split(',') ?? ["default"];
        });

    public static void AddHangfireDashboard(this WebApplication app, IConfiguration configuration) =>
        app.UseHangfireDashboard(configuration.GetValue<string>("Hangfire:Dashboard") ?? "/jobs",
            options: new DashboardOptions
            {
                Authorization = new[] { new NoAuthorizationFilter() },
                DashboardTitle = "ETL Dashboard",
                DisplayStorageConnectionString = true,
                AppPath = null
            });

    public static void ConfigureRecurringJob(this WebApplication app)
    {
        RecurringJob.AddOrUpdate<IServerScraperJob>("Server Scrapper", job => job.ExecuteAsync(null, CancellationToken.None), Cron.Daily(23));
        RecurringJob.AddOrUpdate<ILeaderboardGSheetJob>("Leaderboard GSheet", job => job.ExecuteAsync(null, CancellationToken.None), Cron.Daily(0));
        foreach (var (subregion, index) in Enum.GetValues<SubRegion>().Select((subRegion, index) => (subRegion, index)))
        {
            var region = RegionHelper.SubRegionToRegion(subregion);
            RecurringJob.AddOrUpdate<IFindMinGuildIdServerJob>($"Find min guild id - {subregion}", job => job.ExecuteAsync(null, subregion, CancellationToken.None), $"0 {index} */3 * *");
            RecurringJob.AddOrUpdate<IGuildScraperJob>($"Guild infos - {subregion}", job => job.ExecuteAsync(null, subregion, CancellationToken.None), $"10 {(region == Region.EST ? "6" : "9")} * * 0,2,3,4,6");
            RecurringJob.AddOrUpdate<IPlayerScraperJob>($"Player info top 3 - {subregion}", job => job.ExecuteAsync(null, subregion, true, CancellationToken.None),
                $"30 {(region == Region.EST ? "0,12" : "3,15")} * * 0,2,3,4,6");
            RecurringJob.AddOrUpdate<IPlayerScraperJob>($"Player info full - {subregion}", job => job.ExecuteAsync(null, subregion, false, CancellationToken.None),
                $"30 {(region == Region.EST ? 0 : 4)} * * 1,5");
        }
    }
}