using System.CommandLine;
using Domain.Definitions;

namespace Application.Options;

public static class OptionFactory
{
    public static YamlOption        yamlOption        = new();
    public static InstrumentsOption instrumentsOption = new();
    public static FactorsOption     factorsOption     = new();
    public static StartDateOption   startDateOption   = new();
    public static EndDateOption     endDateOption     = new();
    public static StrategyOption    strategyOption    = new();

    public static Option[] GetAllOptions() => new Option[] 
    { 
        instrumentsOption,
        factorsOption, 
        startDateOption, 
        endDateOption, 
        strategyOption,
        yamlOption
    };
}

