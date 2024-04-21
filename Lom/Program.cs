using Core.Extensions;
using Core.Hangfire.Interfaces;
using Core.Services;
using Core.Services.Interface;
using Jobs.Jobs;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"Environment {builder.Environment.EnvironmentName}");

builder.Services.AddLomConnection(builder.Configuration);

builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddHangfireServer(builder.Configuration);
builder.Services.AddHangfireConsoleExtension();

await builder.Services.AddBrowserService(builder.Configuration);

builder.Services.AddScoped<IFindMinGuildIdServerJob, FindMinGuildIdServerJob>();
builder.Services.AddScoped<IGuildScraperJob, GuildScraperJob>();
builder.Services.AddScoped<IPlayerScraperJob, PlayerScraperJob>();
builder.Services.AddScoped<IServerScraperJob, ServerScraperJob>();
builder.Services.AddHttpClient<IJoyNetClient, JoyNetClient>();

var app = builder.Build();
var browserService = app.Services.GetRequiredService<IBrowserService>();
await browserService.InitializeBrowsers();
app.ConfigureShutdown();
app.AddHangfireDashboard(builder.Configuration);
app.ConfigureRecurringJob();
app.Run(async context => await context.Response.WriteAsync("Starting LOM"));
app.Run();