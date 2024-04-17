using Core.Hangfire;
using Core.Hangfire.Interfaces;
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
        RecurringJob.AddOrUpdate<IServerScrapperJob>("ServerScrapperJob", job => job.ExecuteAsync(null, CancellationToken.None), Cron.Daily(23));
        var enumLength = Enum.GetValues<SubRegion>().Length;
        foreach (var (subregion, index) in Enum.GetValues<SubRegion>().Select((subRegion, index) => (subRegion, index)))
        {
            RecurringJob.AddOrUpdate<IFindMinGuildIdServerJob>($"FindMinGuildIdServerJob-{subregion}", job => job.ExecuteAsync(null, subregion, CancellationToken.None), $"20 {index} */3 * *");
            RecurringJob.AddOrUpdate<IGuildScrapperJob>($"GuildScrapperJob-{subregion}", job => job.ExecuteAsync(null, subregion, CancellationToken.None), $"0 {index} */2 * *");
            RecurringJob.AddOrUpdate<IPlayerScrapperJob>($"PlayerScrapperJob-{subregion}", job => job.ExecuteAsync(null, subregion, true, CancellationToken.None), $"40 {index} * * *");
            RecurringJob.AddOrUpdate<IPlayerScrapperJob>($"PlayerScrapperJob-{subregion}-full", job => job.ExecuteAsync(null, subregion, false, CancellationToken.None), $"10 {index + enumLength} */3 * *");
        }
    }
}