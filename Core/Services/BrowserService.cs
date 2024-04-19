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
    private readonly SemaphoreSlim _semaphoreSlimEu;
    private readonly SemaphoreSlim _semaphoreSlimEst;

    public BrowserService((List<string> euDataPath, List<string> estDataPath) dataPath, ILogger<BrowserService> logger, bool headless = true)
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
        if (_estBrowserLom.Count != 0)
        {
            _logger.LogInformation("Initialized {Count} EST browsers", _estBrowserLom.Count);
            _semaphoreSlimEst = new SemaphoreSlim(_estBrowserLom.Count, _estBrowserLom.Count);
        }
        if (_euBrowserLom.Count != 0)
        {
            _logger.LogInformation("Initialized {Count} EU browsers", _euBrowserLom.Count);
            _semaphoreSlimEu = new SemaphoreSlim(_euBrowserLom.Count, _euBrowserLom.Count);
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
        var semaphoreSlim = region switch
        {
            Region.EU => _semaphoreSlimEu,
            Region.EST => _semaphoreSlimEst,
            _ => throw new ArgumentOutOfRangeException(nameof(region), region, null)
        };
        await semaphoreSlim.WaitAsync();
        var browserLom = region switch
        {
            Region.EU => _euBrowserLom.FirstOrDefault(x => !x.IsInUse),
            Region.EST => _estBrowserLom.FirstOrDefault(x => !x.IsInUse),
            _ => throw new ArgumentOutOfRangeException(nameof(region), region, null)
        };
        if (browserLom is null)
        {
            return null;
        }
        browserLom.LockBrowser();
        _logger.LogDebug("Got browser {BrowserLomId} for {Region}", browserLom.Id, region);
        return browserLom;
    }

    public void ReleaseBrowser(BrowserLom browserLom)
    {
        _logger.LogDebug("Releasing browser {BrowserLomId} for {Region}", browserLom.Id, browserLom.Region);
        browserLom.ReleaseBrowser();
        _ = browserLom.Region switch
        {
            Region.EU => _semaphoreSlimEu.Release(),
            Region.EST => _semaphoreSlimEst.Release(),
            _ => throw new ArgumentOutOfRangeException(nameof(browserLom.Region), browserLom.Region, null)
        };
    }
}