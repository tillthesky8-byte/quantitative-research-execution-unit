using Domain.Interfaces;
using Domain.Models;
using Core.Interfaces;
using Core.States;
using Microsoft.Extensions.Logging;
using Domain.Other;

namespace Core.Simulators;

public class Simulator : ISimulator
{
    private readonly ILogger<Simulator>? _logger;

    private readonly IPortfolio  _portfolio;
    private readonly IStrategy   _strategy;
    private readonly IBroker     _broker;
    private readonly IRecorder   _recorder;
    private readonly MarketState _marketState;

    public Simulator(IPortfolio portfolio, IStrategy strategy, IBroker broker, IRecorder recorder, ILogger<Simulator>? logger)
    {
        _portfolio   = portfolio;
        _strategy    = strategy;
        _broker      = broker;
        _recorder    = recorder;
        _logger      = logger;
        _marketState = new MarketState();
    }

    public async Task<SimulationResult> Run(IStreamer streamer)
    {
        long? previousTimestamp = null; 

        await foreach (var marketEvent in streamer.StreamAsync())
        {
            if (previousTimestamp is not null && marketEvent.Timestamp > previousTimestamp)
            {
                _marketState.Timestamp = previousTimestamp.Value;
                DecisionCycle();
                _logger?.LogTrace(LogMessages.PortfolioSummaryAtTimestamp, previousTimestamp, _portfolio.Cash, _portfolio.GetEquity(_marketState), _portfolio.Positions.Count, _portfolio.RealizedPnlEvents.Count());
            }
            _marketState.UpdateState(marketEvent);
            previousTimestamp = marketEvent.Timestamp;
        }

        if (previousTimestamp is not null)
        {
            _marketState.Timestamp = previousTimestamp.Value;
            DecisionCycle();
        }

        _recorder.AppendTrades(_broker.TradeRecords);
        _recorder.AppendRealizedPnlEvents(_portfolio.RealizedPnlEvents);
        return _recorder.BuildResult();
    }

    private void DecisionCycle()
    {
        _broker.ProcessOrders(_marketState, _portfolio);
        var orderRequests = _strategy.OnTick(_portfolio, _marketState);
        foreach (var orderRequest in orderRequests)
            _broker.SubmitOrder(orderRequest, _portfolio, _marketState.Timestamp);
    
        _recorder.Record(_marketState.Timestamp, _portfolio.GetEquity(_marketState));
    }

}