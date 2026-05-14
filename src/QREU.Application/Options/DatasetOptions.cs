using Domain.Definitions;
using System.CommandLine;

public sealed class InstrumentsOption : Option<InstrumentDefinition[]>
{
    public InstrumentsOption() : base("--instrument", "-i")
    {
        Arity = ArgumentArity.OneOrMore;
        AllowMultipleArgumentsPerToken = true;
        Required = true;
        Description = "Defines an instrument to be included in the dataset. Can be specified multiple instruments, e.g., --instruments AAPL MSFT GOOGL or --instruments AAPL --instruments MSFT --instruments GOOGL.";
        CustomParser = result =>
        {
            var instrumentArgs = result.Tokens.Select(t => t.Value).ToArray();
            if (instrumentArgs.Length == 0)
                throw new ArgumentException("At least one instrument must be specified using the --instrument option.");

            var instrumentDefinitions = new List<InstrumentDefinition>();
            foreach (var arg in instrumentArgs)
            {
                var instrumentDef = new InstrumentDefinition { Symbol = arg.Trim() };
                instrumentDefinitions.Add(instrumentDef);
            }
            return [.. instrumentDefinitions];
        };
    }
}

public sealed class FactorsOption : Option<FactorDefinition[]>
{
    public FactorsOption() : base("--factor", "-fa")
    {
        Arity = ArgumentArity.ZeroOrMore;
        AllowMultipleArgumentsPerToken = true;
        Description = "Defines a factor to be included in the dataset. Can be specified multiple factors, e.g., --factors global:us_interest_rate msft:eps or --factors global:us_interest_rate --factors msft:eps.";
        CustomParser = result =>
        {
            var factorArgs = result.Tokens.Select(t => t.Value).ToArray();
            var factorDefinitions = new List<FactorDefinition>();
            foreach (var arg in factorArgs)
            {
                var factorDef = ParseFactorArgument(arg);
                factorDefinitions.Add(factorDef);
            }
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