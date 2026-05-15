using Domain.Enums;
using Microsoft.Extensions.Configuration;
namespace Application.Models;

public sealed class AppSettings
{
    public string ConnectionString { get; init; } 
    public string ConfigurationRoot { get; init; }
    public decimal InitialCash { get; init; } 
    public SlippageType SlippageType { get; init; }
    public CommissionType CommissionType { get; init; }

    public AppSettings(ConfigurationManager configuration)
    {
        ConnectionString  = configuration.GetConnectionString("DuckDb")                           ?? throw new InvalidOperationException("Connection string 'DuckDb' not found.");
        ConfigurationRoot = configuration["Paths:ConfigurationRoot"]                              ?? throw new InvalidOperationException("Configuration path 'Paths:ConfigurationRoot' not found.");
        InitialCash       = decimal.Parse(configuration["Simulation:InitialCash"]                 ?? throw new InvalidOperationException("Initial cash value not found."));
        SlippageType      = Enum.Parse<SlippageType>(configuration["Simulation:SlippageModel"]     ?? throw new InvalidOperationException("Slippage type not found."));
        CommissionType    = Enum.Parse<CommissionType>(configuration["Simulation:CommissionModel"] ?? throw new InvalidOperationException("Commission type not found."));
    }
}
