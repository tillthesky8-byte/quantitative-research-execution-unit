namespace Domain.Definitions;

public sealed class StrategyDefinition
{
    public required StrategyType Type { get; init; }
    public required Dictionary<string, string> Parameters { get; init; }
}