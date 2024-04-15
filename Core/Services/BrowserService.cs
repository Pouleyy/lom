using Core.Services.Models;
using Microsoft.Extensions.Logging;
using PuppeteerSharp;

namespace Core.Services;

public class BrowserService
{
    private readonly IPage _page;
    
    public EventHandler<ConsoleMessageEvent> ConsoleMessageEvent { get; set; }
    
    public BrowserService(IPage page, ILogger<BrowserService> logger)
    {
        _page = page;
        _page.Console += ConsoleMessageReceived;
    }

    private void ConsoleMessageReceived(object? sender, ConsoleEventArgs e)
    {
        Console.WriteLine($"Console message: {e.Message.Text}");
        ConsoleMessageEvent?.Invoke(this, new ConsoleMessageEvent
        {
            Message = e.Message.Text,
            Response = e.Message.Args?.Last()
        });
    }
    
    public async Task WriteToConsole(string message)
    {
        await _page.EvaluateExpressionAsync(message);
    }
    
    public async Task ChangePrintLevel()
    {
        await _page.EvaluateExpressionAsync("GlobalDefine.PRINT_LEVEL = 0");
    }
    
}