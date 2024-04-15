using Entities.Models;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Entities.Helper;

public class ServerShortNameConverter() : ValueConverter<ServerShortName, string>(e => e.ToString(),
    s => Enum.Parse<ServerShortName>(s));