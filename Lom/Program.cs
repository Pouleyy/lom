using Core.Extensions;
using Core.Services;
using Core.Services.Interface;
using Hangfire;
using Jobs.Jobs;

var builder = WebApplication.CreateBuilder(args);
Console.WriteLine($"Environment {builder.Environment.EnvironmentName}");

builder.Services.AddLomConnection(builder.Configuration);

builder.Services.AddHangfire(builder.Configuration);
builder.Services.AddHangfireServer(builder.Configuration);
builder.Services.AddHangfireConsoleExtension();
builder.Services.AddHttpClient<IJoyNetClient, JoyNetClient>();

var app = builder.Build();
app.AddHangfireDashboard(builder.Configuration);
app.ConfigureRecurringJob();
BackgroundJob.Enqueue<ServerParseJob>(job => job.ExecuteAsync(null, CancellationToken.None));
app.Run(async context => await context.Response.WriteAsync("Starting LOM"));
app.Run();