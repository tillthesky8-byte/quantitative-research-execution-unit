using Domain.Definitions;
namespace Studio.Web.Models;

public record ConfigJson
(
    Guid RunId,
    DateTime RanAt,
    string DatasetHash,
    string StrategyHash,
    string ConnectionString,
    DatasetDefinition Dataset,
    SimulatorDefinition Simulator,
    bool IncludeEquityCurve,
    bool IncludeTrades,
    bool IncludePnlEvents
);