using Core.Services.Interface;
using Core.Services.Models;
using Entities.Models;
using Microsoft.Extensions.Logging;

namespace Core.Services;

public class BrowserService : IBrowserService
{
    private readonly ILogger<IBrowserService> _logger;
    private readonly List<BrowserLom> _euBrowserLom = [];
    private readonly List<BrowserLom> _estBrowserLom = [];

    public BrowserService((List<string> euDataPath, List<string> estDataPath) dataPath, ILogger<IBrowserService> logger, bool headless = true)
    {
        _logger = logger;
        foreach (var (euDataPath, index) in dataPath.euDataPath.Select((value, index) => (value, index)))
        {
             _euBrowserLom.Add(new BrowserLom(euDataPath, headless, index, Region.EU));
        }
        foreach (var (estDataPath, index) in dataPath.estDataPath.Select((value, index) => (value, index)))
        {
            _estBrowserLom.Add(new BrowserLom(estDataPath, headless, index, Region.EST));
        }
    }
    
    public async Task InitializeBrowsers()
    {
        _logger.LogInformation("Initializing browsers");
        var tasks = _euBrowserLom.Concat(_estBrowserLom).Select(browserLom => browserLom.Initialize()).ToList();
        await Task.WhenAll(tasks);
        _logger.LogInformation("Browsers initialized");
    }
    
    public async Task<BrowserLom?> GetBrowser(Region region)
    {
        _logger.LogDebug("Getting browser for {Region}", region);
        var browserLom = region switch
        {
            Region.EU => _euBrowserLom.FirstOrDefault(x => x.Lock.CurrentCount == 1),
            Region.EST => _estBrowserLom.FirstOrDefault(x => x.Lock.CurrentCount == 1),
            _ => throw new ArgumentOutOfRangeException(nameof(region), region, null)
        };
        if (browserLom is null)
        {
            return null;
        }
        await browserLom.Lock.WaitAsync();
        _logger.LogDebug("Got browser {BrowserLomId} for {Region}", browserLom.Id, region);
        return browserLom;
    }
    
    public void ReleaseBrowser(BrowserLom browserLom)
    {
        _logger.LogDebug("Releasing browser {BrowserLomId} for {Region}", browserLom.Id, browserLom.Region);
        browserLom.Lock.Release();
    }
}