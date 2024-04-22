using Entities.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Entities.Helper;

public class ServerShortNameConverter() : ValueConverter<SubRegion, string>(e => e.ToString(),
    s => Enum.Parse<SubRegion>(s));