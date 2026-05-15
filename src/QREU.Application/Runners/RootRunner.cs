using Core.Factories;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Writer;

namespace Application.Runners;

public sealed class RootRunner
{
    private readonly RunConfiguration _configuration;
    private readonly ILogger<RootRunner>? _logger;
    private readonly ILoggerFactory _loggerFactory;

    public RootRunner(RunConfiguration configuration, ILogger<RootRunner>? logger = null, ILoggerFactory? loggerFactory = null)
    {
        _configuration = configuration;
        _logger = logger;
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }
    public async Task Run()
    {
        _logger?.LogInformation("Starting RootRunner with configuration: {@Configuration}", _configuration);
        //====================================================
        // BUILD COMPONENTS
        //====================================================
    
        await using var streamer = await new StreamerFactory(
            _configuration.ConnectionString, 
            _configuration.Dataset, 
            _loggerFactory,
            _loggerFactory.CreateLogger<StreamerFactory>()
        ).CreateStreamer();


        var simulator = new SimulatorBuilder(
            _configuration.Simulator,
            _loggerFactory,
            _loggerFactory.CreateLogger<SimulatorBuilder>()
        ).CreateSimulator();

        var writer = new WriteManager(
            _configuration, 
            _configuration.ConnectionString
        );


        //====================================================
        // ROOT EXECUTION
        //====================================================

        var result = await simulator.Run(streamer);


        //====================================================
        // REPORTING
        //====================================================

        Console.WriteLine(result.ToString());
        _logger?.LogInformation("RootRunner completed successfully.");

        await writer.WriteDataAsync(result);
        _logger?.LogInformation("Results written to database successfully.");
    }
}