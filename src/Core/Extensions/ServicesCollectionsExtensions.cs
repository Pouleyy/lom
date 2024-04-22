using Core.Services;
using Core.Services.Interface;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Core.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static async Task<IServiceCollection> AddBrowserService(this IServiceCollection services, IConfiguration configuration)
    {
        var dataPath = (euDataPath: configuration.GetSection("Puppeteer:EuDataPath").Get<List<string>>() ?? throw new ArgumentNullException("Missing BrowserService EuDataPath in configuration"),
                        estDataPath: configuration.GetSection("Puppeteer:EstDataPath").Get<List<string>>() ?? []);
        var headless = configuration.GetValue<bool>("Puppeteer:Headless");
        services.AddSingleton<IBrowserService, BrowserService>(provider =>
        {
            var browserService = new BrowserService(dataPath, provider.GetRequiredService<ILogger<BrowserService>>(), headless);
            return browserService;
        });
        return services;
    }

    public static void ConfigureShutdown(this IApplicationBuilder app)
    {
        var applicationLifetime = app.ApplicationServices.GetRequiredService<IHostApplicationLifetime>();
        var browserService = app.ApplicationServices.GetRequiredService<IBrowserService>();

        applicationLifetime.ApplicationStopping.Register(() =>
        {
            browserService.Dispose();
        });
    }
}