using Microsoft.Extensions.Configuration;

namespace ReliableBack.Infrastructure.Configuration;

public static class ConfigurationExtensions
{
    public static TSettings GetRequiredSettings<TSettings>(this IConfiguration configuration, string sectionName)
        where TSettings : class
    {
        return configuration
                   .GetRequiredSection(sectionName)
                   .Get<TSettings>()
               ?? throw new InvalidOperationException($"Configuration section '{sectionName}' is not configured.");
    }
}