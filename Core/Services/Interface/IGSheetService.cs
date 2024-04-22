using Core.Services.Models;

namespace Core.Services.Interface;

public interface IGSheetService
{
    Task WriteTop10Guilds(List<IEnumerable<FamilyLeadboard>> families, CancellationToken cancellationToken = default);
}