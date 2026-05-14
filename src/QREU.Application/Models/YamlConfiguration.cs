using Domain.Definitions;

public class YamlConfiguration
{
    public string? Name { get; set; }
    public List<string>? Instruments { get; set; }
    public List<YamlFactorDefinition>? Factors { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public StrategyDefinition? Strategy { get; set; }

}

public class YamlStrategyDefinition
{
    public StrategyType?               Type { get; set; }
    public Dictionary<string, string>? Parameters { get; set; }
}

public class YamlFactorDefinition
{
    public string? Symbol { get; set; }
    public string? Name { get; set; }
}