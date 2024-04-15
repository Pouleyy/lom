using PuppeteerSharp;

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
    UserDataDir = "C:\\dev\\Lom\\lom",
    
});
var page = await browser.NewPageAsync();
await page.GoToAsync("https://lom.joynetgame.com/");
//listen to console log messages
page.Console += (sender, e) => Console.WriteLine($"{e.Message.Text}");
Console.ReadLine();