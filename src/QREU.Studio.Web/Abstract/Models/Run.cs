namespace Studio.Web.Models;

public record Run
(
    Guid         Id,
    DateTime     RanAt,
    string       StrategyName,
    ConfigJson   ConfigJson,
    string       MetricsJson
);
    