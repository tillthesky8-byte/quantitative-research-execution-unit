using Domain.Definitions;
using System.CommandLine;
namespace Application.Options;
public sealed class InstrumentsOption : Option<InstrumentDefinition[]?>
{
    public InstrumentsOption() : base("--instrument", "-i")
    {
        Arity = ArgumentArity.ZeroOrMore;
        AllowMultipleArgumentsPerToken = true;
        DefaultValueFactory = null;
        Description = "Defines an instrument to be included in the dataset. Can be specified multiple instruments, e.g., --instruments AAPL MSFT GOOGL or --instruments AAPL --instruments MSFT --instruments GOOGL.";
        CustomParser = result =>
        {
            var instrumentArgs = result.Tokens.Select(t => t.Value).ToArray();
            if (instrumentArgs.Length == 0)
                return null;

            var instrumentDefinitions = new List<InstrumentDefinition>();
            foreach (var arg in instrumentArgs)
            {
                var instrumentDef = new InstrumentDefinition { Symbol = arg.Trim() };
                instrumentDefinitions.Add(instrumentDef);
            }

            if (instrumentDefinitions.Count == 0)
                return null;
            
            Console.WriteLine($"Parsed {instrumentDefinitions.Count} instruments: {string.Join(", ", instrumentDefinitions.Select(i => i.Symbol))}");

            return [.. instrumentDefinitions];
        };
    }
}

public sealed class FactorsOption : Option<FactorDefinition[]?>
{
    public FactorsOption() : base("--factor", "-fa")
    {
        Arity = ArgumentArity.ZeroOrMore;
        AllowMultipleArgumentsPerToken = true;
        DefaultValueFactory = null;
        Description = "Defines a factor to be included in the dataset. Can be specified multiple factors, e.g., --factors global:us_interest_rate msft:eps or --factors global:us_interest_rate --factors msft:eps.";
        CustomParser = result =>
        {
            var factorArgs = result.Tokens.Select(t => t.Value).ToArray();
            if (factorArgs.Length == 0)
                return null;

            var factorDefinitions = new List<FactorDefinition>();
            foreach (var arg in factorArgs)
            {
                var factorDef = ParseFactorArgument(arg);
                factorDefinitions.Add(factorDef);
            }
            if (factorDefinitions.Count == 0)
                return null;
            return [.. factorDefinitions];
        };
    }

    private FactorDefinition ParseFactorArgument(string arg)
    {
        var parts = arg.Split(':', 2);
        if (parts.Length != 2)
            throw new ArgumentException($"Invalid factor definition '{arg}'. Expected format: symbol:name (e.g., msft:ebitda).");
        if (string.IsNullOrWhiteSpace(parts[0]) || string.IsNullOrWhiteSpace(parts[1]))
            throw new ArgumentException($"Invalid factor definition '{arg}'. Expected format: symbol:name (e.g., msft:ebitda).");

        var symbol = parts[0].Trim();
        var name = parts[1].Trim();
        return new FactorDefinition { Symbol = symbol, Name = name};
    }
}