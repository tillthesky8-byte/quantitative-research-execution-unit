using Domain.Interfaces;
using Domain.Models;
using Modules.Families;
using Modules.Indicators;
using Core.Interfaces;

namespace Modules.Strategies;


public sealed class CustomBBB : ExitEntryStrategy, IStrategy
{
    private readonly CustomBollingerBands _bb;
    protected override IIndicator[] Indicators =>[ _bb ];
    protected override bool OpenLongCondition(SymbolState symbolState) => 
        symbolState.Close < _bb.LowerBand(symbolState.Symbol) && _bb.IsReady;
    protected override bool OpenShortCondition(SymbolState symbolState) => 
        symbolState.Close > _bb.UpperBand(symbolState.Symbol) && _bb.IsReady;
    protected override bool CloseLongCondition(SymbolState symbolState) => 
        symbolState.Close > _bb.MiddleBand(symbolState.Symbol);
    protected override bool CloseShortCondition(SymbolState symbolState) => 
        symbolState.Close < _bb.MiddleBand(symbolState.Symbol);


    public CustomBBB(IReadOnlyDictionary<string, string> parametersRaw)
    {
        var parameters = parametersRaw.ToDictionary(p => p.Key, p => p.Value, StringComparer.OrdinalIgnoreCase);

        var middlePeriod = parameters.TryGetValue("period", out var periodStr) 
            ? int.Parse(periodStr) : throw new ArgumentException("Missing required parameter: period");

        var sidePeriod = parameters.TryGetValue("sideperiod", out var sidePeriodStr) 
            ? int.Parse(sidePeriodStr) : throw new ArgumentException("Missing required parameter: sideperiod");

        var stdDevMultiplier = parameters.TryGetValue("stdm", out var stdDevStr)
            ? decimal.Parse(stdDevStr) : throw new ArgumentException("Missing required parameter: stdm");
        
        var source = parameters.TryGetValue("source", out var sourceStr)
            ? sourceStr : throw new ArgumentException("Missing required parameter: source");

        _bb = new CustomBollingerBands(middlePeriod, sidePeriod, stdDevMultiplier, source);
    }
}