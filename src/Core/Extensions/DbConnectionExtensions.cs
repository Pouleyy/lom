using Entities.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Extensions;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddLomConnection(this IServiceCollection services, IConfiguration configuration)
    {
        var lomConnectionString = configuration.GetConnectionString("Lom") ?? string.Empty;
        if (string.IsNullOrWhiteSpace(lomConnectionString))
        {
            throw new ArgumentNullException(nameof(lomConnectionString), "Lom connection string not specified in configuration");
        }
        services.AddDbContext<LomDbContext>(options => options.UseNpgsql(lomConnectionString));
        return services;
    }
}