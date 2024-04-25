using Core.Services.Models;
using Entities.Models;

namespace Core.Services.Interface;

public interface IGSheetService
{
    Task WriteTop10Guilds(List<IEnumerable<FamilyLeadboard>> families, CancellationToken cancellationToken = default);
    Task WriteLastExecutionTimeBySubRegion(Dictionary<SubRegion, (long full, long top3)> lastExecutionTimeBySubRegion, CancellationToken cancellationToken);
}