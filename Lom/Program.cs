using Core.Extensions;
using Core.Services;
using Core.Services.Interface;
using Hangfire;
using Jobs.Jobs;
using PuppeteerSharp;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"Environment {builder.Environment.EnvironmentName}");

using var browserFetcher = new BrowserFetcher();
await browserFetcher.DownloadAsync();
var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = false,
    Args = ["--window-size=585,1039", "--disable-save-password-bubble"],
    DefaultViewport = new ViewPortOptions()
    {
        Height = 1039,
        Width = 585
    },
    Devtools = true,
    UserDataDir = "C:\\dev\\Lom\\lom-data",
    
});
var page = await browser.NewPageAsync();
await page.GoToAsync("https://lom.joynetgame.com/");

builder.Services.AddSingleton<BrowserService>(services => new BrowserService(page, services.GetRequiredService<ILogger<BrowserService>>()));
builder.Services.AddLomConnection(builder.Configuration);

builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddHangfireServer(builder.Configuration);
builder.Services.AddHangfireConsoleExtension();
builder.Services.AddHttpClient<IJoyNetClient, JoyNetClient>();

var app = builder.Build();
app.AddHangfireDashboard(builder.Configuration);
app.ConfigureRecurringJob();
BackgroundJob.Enqueue<FindMinGuildIdServerJob>(job => job.ExecuteAsync(null, 30011, CancellationToken.None));
app.Run(async context => await context.Response.WriteAsync("Starting LOM"));
app.Run();