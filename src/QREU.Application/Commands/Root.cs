using System.CommandLine;
using Application.Models;
using Application.Options;
using Application.Runners;
using Domain.Definitions;
using Domain.Models;
using Domain.Other;
using Microsoft.Extensions.Logging;

public sealed class Root : RootCommand
{
    public Root(AppSettings appSettings, ILogger<Root> logger, ILoggerFactory loggerFactory) : base("QREU - Quantitative Research Execution Unit")
    {
        foreach (var option in OptionFactory.GetAllOptions()) Add(option);

        SetAction(async (context) =>
        {
            var yaml = context.GetValue(OptionFactory.yamlOption);

            if (!string.IsNullOrEmpty(yaml))
                YamlFactory.LoadFromYaml(appSettings.ConfigurationRoot, yaml);
            
            var instruments = (context.GetValue(OptionFactory.instrumentsOption)?.Length == 0) 
                ? YamlFactory.Instruments : context.GetValue(OptionFactory.instrumentsOption) 
                ?? throw new InvalidOperationException("Instruments option is required but was not provided.");

            var factors     = (context.GetValue(OptionFactory.factorsOption)?.Length == 0)    
                ? (YamlFactory.Factors ?? Array.Empty<FactorDefinition>())
                : context.GetValue(OptionFactory.factorsOption)
                ?? Array.Empty<FactorDefinition>();

            var startDate   = context.GetValue(OptionFactory.startDateOption)   ?? YamlFactory.StartDate   ?? DateTime.MinValue;
           
            var endDate     = context.GetValue(OptionFactory.endDateOption)     ?? YamlFactory.EndDate     ?? DateTime.MaxValue;
           
            var strategy    = context.GetValue(OptionFactory.strategyOption)    ?? YamlFactory.Strategy    ?? throw new InvalidOperationException("Strategy option is required but was not provided.");

            var dataset = new DatasetDefinition
                (
                    Instruments : instruments!,
                    Factors     : factors!,
                    StartDate   : startDate,
                    EndDate     : endDate
                );

            var simulator = new SimulatorDefinition
                (
                    Strategy       : strategy,
                    SlippageType   : appSettings.SlippageType,
                    CommissionType : appSettings.CommissionType,
                    InitialCash    : appSettings.InitialCash
                );

            var runConfiguration = new RunConfiguration(appSettings.ConnectionString, dataset, simulator);

            var runner = new RootRunner(runConfiguration, loggerFactory.CreateLogger<RootRunner>(), loggerFactory);

            await runner.Run();
        });   
    }
}