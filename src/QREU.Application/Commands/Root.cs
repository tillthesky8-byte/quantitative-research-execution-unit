using System.CommandLine;
using Application.Models;
using Application.Options;
using Domain.Definitions;
using Microsoft.Extensions.Logging;

public sealed class Root : RootCommand
{
    public Root(AppSettings appSettings, ILogger<Root> logger, ILoggerFactory loggerFactory) : base("QREU - Quantitative Research Execution Unit")
    {
        foreach (var option in OptionFactory.GetAllOptions()) Add(option);
        SetAction(async (context) =>
        {
            var instruments = context.GetValue(OptionFactory.instrumentsOption) ?? throw new InvalidOperationException("Instruments option is required but was not provided.");
            var factors     = context.GetValue(OptionFactory.factorsOption)     ?? Array.Empty<FactorDefinition>();
            var startDate   = context.GetValue(OptionFactory.startDateOption)   ?? DateTime.MinValue;
            var endDate     = context.GetValue(OptionFactory.endDateOption)     ?? DateTime.MaxValue;
            var strategy    = context.GetValue(OptionFactory.strategyOption)    ?? throw new InvalidOperationException("Strategy option is required but was not provided.");

        });
    }
}