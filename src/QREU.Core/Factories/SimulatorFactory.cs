using Core.Interfaces;
using Core.Recording;
using Core.Simulators;
using Core.States;
using Domain.Definitions;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace Core.Factories;

public class SimulatorBuilder
{
    private readonly ILogger<SimulatorBuilder>? _logger;
    private readonly ILoggerFactory _loggerFactory;
    private readonly SimulatorDefinition _simulatorDefinition;
    public SimulatorBuilder(SimulatorDefinition simulatorDefinition, ILoggerFactory loggerFactory, ILogger<SimulatorBuilder>? logger)
    {
        _simulatorDefinition = simulatorDefinition;
        _loggerFactory       = loggerFactory;
        _logger              = logger;
    }
    public ISimulator CreateSimulator()
    {
        var strategy = StrategyFactory.CreateStrategy(_simulatorDefinition.Strategy.Type, _simulatorDefinition.Strategy.Parameters);
        var broker   = BrokerFactory.CreateBroker(_simulatorDefinition.SlippageType, _simulatorDefinition.CommissionType, _loggerFactory);
        
        var portfolio = new Portfolio(_loggerFactory.CreateLogger<Portfolio>(), _simulatorDefinition.InitialCash);
        var recorder  = new Recorder();
        
        return new Simulator(portfolio, strategy, broker, recorder, _loggerFactory.CreateLogger<Simulator>());

    }
}