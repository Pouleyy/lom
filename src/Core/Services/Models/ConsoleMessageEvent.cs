using PuppeteerSharp;

namespace Core.Services.Models;

public class ConsoleMessageEvent : EventArgs
{
    public string Message { get; set; }
    public IJSHandle? Response { get; set; }
}