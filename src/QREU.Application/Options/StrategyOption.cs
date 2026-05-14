using System.CommandLine;
using Domain.Definitions;

namespace Application.Options;
public sealed class StrategyOption : Option<StrategyDefinition>
{
    public StrategyOption() : base("--strategy", "-sg")
    {
        Description = "Defines the trading strategy to be used in the simulation. Specify the strategy name followed by any parameters (e.g., --strategy meanReversion:lookback=20,threshold=0.05).";
        Arity = ArgumentArity.ExactlyOne;
        Required = true;
        CustomParser = result =>
        {
            var strategyArg = result.Tokens.Select(t => t.Value).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(strategyArg))
                throw new ArgumentException("Strategy definition is required. Expected format: strategyType:param1=value1,param2=value2 (e.g., meanReversion:lookback=20,threshold=0.05).");
            return ParseStrategyArgument(strategyArg);
        };
    }

    private StrategyDefinition ParseStrategyArgument(string arg)
    {
        var parts = arg.Split(':', 2);
        if (string.IsNullOrWhiteSpace(parts[0]))
            throw new ArgumentException($"Invalid strategy definition '{arg}'. Expected format: strategyType:param1=value1,param2=value2 (e.g., meanReversion:lookback=20,threshold=0.05).");

        var typeString = parts[0].Trim();
        var type = Enum.TryParse<StrategyType>(typeString, true, out var parsedType) ? parsedType : throw new ArgumentException($"Invalid strategy type '{typeString}' in strategy definition. Valid types are: {string.Join(", ", Enum.GetNames<StrategyType>())}.");
        var parameters = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (parts.Length > 1)
        {
            var paramPairs = parts[1].Split(',', StringSplitOptions.RemoveEmptyEntries);
            foreach (var pair in paramPairs)
            {
                var kv = pair.Split('=', 2);
                if (kv.Length != 2 || string.IsNullOrWhiteSpace(kv[0]) || string.IsNullOrWhiteSpace(kv[1]))
                    throw new ArgumentException($"Invalid parameter definition '{pair}' in strategy argument. Expected format: key=value (e.g., lookback=20).");

                parameters[kv[0].Trim()] = kv[1].Trim();
            }
        }     
        return new StrategyDefinition { Type = type, Parameters = parameters };   
    }
    
}