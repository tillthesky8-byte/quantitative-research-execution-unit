using Domain.Definitions;
using System.Security.Cryptography;
        namespace Domain.Models;
public sealed class RunConfiguration
{
    public Guid RunId { get; set; }
    public DateTime RanAt { get; set; }
    public string DatasetHash  { get; set; }
    public string StrategyHash { get; set; }
    public string                 ConnectionString { get; init; }
    public DatasetDefinition      Dataset { get; init; }
    public SimulatorDefinition    Simulator { get; init; }

    public bool IncludeEquityCurve { get; init; } = true;
    public bool IncludeTrades { get; init; } = true;
    public bool IncludePnlEvents { get; init; } = true;


    public RunConfiguration(string connectionString, DatasetDefinition dataset, SimulatorDefinition simulator)
    {
        ConnectionString = connectionString;
        Dataset          = dataset;
        Simulator        = simulator;

        RunId        = Guid.NewGuid();
        RanAt        = DateTime.UtcNow;
        DatasetHash  = Dataset.BuildHash();
        StrategyHash = Simulator.Strategy.BuildHash();
    }

}