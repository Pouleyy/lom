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
        var message = e.Message.Text.Split(' ');
        if(message.Length < 3) return;
        ConsoleMessageEvent?.Invoke(this, new ConsoleMessageEvent
        {
            Message = message[2],
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