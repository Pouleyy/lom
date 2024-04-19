using Entities.Models;
using PuppeteerSharp;

namespace Core.Services.Models;

public class BrowserLom(string path, bool headless, int id, Region region) : IDisposable
{
    private int _isInUseFlag = 0;

    public bool IsInUse => _isInUseFlag == 1;
    
    public EventHandler<ConsoleMessageEvent>? ConsoleMessageEvent { get; set; }
    public int Id { get; } = id;
    public Region Region { get; } = region;

    private IBrowser? _browser;
    private IPage? _page;
    private bool _initialized;

    public async Task Initialize()
    {
        if (_initialized) return;
        _ = new BrowserFetcher().DownloadAsync().Result;

        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = headless,
            Args = ["--window-size=585,1039", "--disable-save-password-bubble"],
            DefaultViewport = new ViewPortOptions()
            {
                Height = 1039,
                Width = 585
            },
            Devtools = false,
            UserDataDir = path,
        });
        _page = (await _browser.PagesAsync()).First();
        await _page.GoToAsync("https://lom.joynetgame.com/");
        await Task.Delay(8000);
        await ChangePrintLevel();
        _page.Console += ConsoleMessageReceived;
        _initialized = true;
    }

    public async Task WriteToConsole(string message)
    {
        await _page!.EvaluateExpressionAsync(message);
    }

    public void LockBrowser() => Interlocked.Exchange(ref _isInUseFlag, 1);
    
    public void ReleaseBrowser() => Interlocked.Exchange(ref _isInUseFlag, 0);
    
    private async Task ChangePrintLevel()
    {
        await _page!.EvaluateExpressionAsync("GlobalDefine.PRINT_LEVEL = 0");
    }

    private void ConsoleMessageReceived(object? sender, ConsoleEventArgs e)
    {
        var message = e.Message.Text.Split(' ');
        if (message.Length < 3) return;
        ConsoleMessageEvent?.Invoke(this, new ConsoleMessageEvent
        {
            Message = message[2],
            Response = e.Message.Args?.Last()
        });
    }

    public void Dispose() => _browser?.Dispose();
}