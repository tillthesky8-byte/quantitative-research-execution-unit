using Domain.Definitions;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Application.Options;

public static class YamlFactory
{
    private static readonly IDeserializer deserializer = new DeserializerBuilder()
        .WithNamingConvention(CamelCaseNamingConvention.Instance)
        .Build();

    public static InstrumentDefinition[]? Instruments { get; private set; }
    public static FactorDefinition[]?     Factors     { get; private set; }
    public static DateTime?               StartDate   { get; private set; }
    public static DateTime?               EndDate     { get; private set; }
    public static StrategyDefinition?     Strategy    { get; private set; }

    public static void LoadFromYaml(string configRoot, string configName)
    {
        var yamlPath = Path.Combine(configRoot, $"{configName}.yaml");
        if (!File.Exists(yamlPath))
        {
            throw new FileNotFoundException($"YAML configuration file not found at path: {yamlPath}");
        }

        var yamlContent = File.ReadAllText(yamlPath);
        var yamlConfig = deserializer.Deserialize<YamlConfiguration>(yamlContent);

        if (yamlConfig == null)
        {
            throw new InvalidDataException($"Failed to deserialize YAML configuration from file: {yamlPath}");
        }

        Instruments = yamlConfig.Instruments?.Select(symbol => new InstrumentDefinition { Symbol = symbol }).ToArray();
        Factors     = yamlConfig.Factors?    .Select(f => new FactorDefinition { Symbol = f.Symbol!, Name = f.Name! }).ToArray();
        StartDate   = yamlConfig.StartDate;
        EndDate     = yamlConfig.EndDate;
        Strategy    = yamlConfig.Strategy != null
            ? new StrategyDefinition
            {
                Type       = yamlConfig.Strategy.Type,
                Parameters = yamlConfig.Strategy.Parameters 
            }
            : null;
    }
}