namespace Studio.Web.Models;

public record RawRun
(
    Guid         Id,
    DateTime     RanAt,
    string       StrategyName,
    ConfigJson   ConfigJson,
    string       MetricsJson
);
    
public record Run
(
    Guid         Id,
    DateTime     RanAt,
    string       StrategyName,
    string[]     Symbols,
    DateTime     StartDate,
    DateTime     EndDate
);