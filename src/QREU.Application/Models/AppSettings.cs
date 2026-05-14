using Microsoft.Extensions.Configuration;
namespace Application.Models;

public sealed class AppSettings
{
    public string ConnectionString { get; init; } 
    public string ConfigurationRoot { get; init; }

    public AppSettings(ConfigurationManager configuration)
    {
        ConnectionString  = configuration.GetConnectionString("DuckDb") ?? throw new InvalidOperationException("Connection string 'DuckDb' not found.");
        ConfigurationRoot = configuration["Paths:ConfigurationRoot"]    ?? throw new InvalidOperationException("Configuration path 'Paths:ConfigurationRoot' not found.");
    }
}
