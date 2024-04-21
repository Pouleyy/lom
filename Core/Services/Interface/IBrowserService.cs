using Core.Services.Models;
using Entities.Models;

namespace Core.Services.Interface;

public interface IBrowserService
{
    Task InitializeBrowsers();
    Task<BrowserLom?> GetBrowser(Region region, CancellationToken cancellationToken = default);
    void ReleaseBrowser(BrowserLom browserLom);
    void Dispose();
}