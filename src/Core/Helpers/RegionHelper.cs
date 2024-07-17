using Entities.Models;

namespace Core.Helpers;

public static class RegionHelper
{
    public static Region SubRegionToRegion(SubRegion subRegion)
    {
        return subRegion switch
        {
            SubRegion.AMEN => Region.EST,
            SubRegion.ES => Region.EST,
            SubRegion.PT => Region.EST,
            SubRegion.EUEN => Region.EU,
            SubRegion.DE => Region.EU,
            SubRegion.FR => Region.EU,
            SubRegion.TR => Region.EU,
            SubRegion.RU => Region.EU,
            SubRegion.ME => Region.EU,
            _ => throw new ArgumentOutOfRangeException(nameof(subRegion), subRegion, null)
        };
    }
}