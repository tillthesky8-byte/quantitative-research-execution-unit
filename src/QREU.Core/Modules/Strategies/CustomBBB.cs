using Domain.Interfaces;
using Domain.Models;
using Modules.Families;
using Modules.Indicators;
using Core.Interfaces;

namespace Modules.Strategies;


public sealed class CustomBBB : ExitEntryStrategy, IStrategy
{
    private readonly CustomBollingerBands _bollingerBands;
    protected override IIndicator[] Indicators =>[ _bollingerBands ];
    protected override bool OpenLongCondition(SymbolState symbolState) => 
        (symbolState.Close > _bollingerBands.UpperBand(symbolState.Symbol) || symbolState.Close < _bollingerBands.LowerBand(symbolState.Symbol)) && _bollingerBands.IsReady;
    protected override bool OpenShortCondition(SymbolState symbolState) => 
        false;
    protected override bool CloseLongCondition(SymbolState symbolState) => 
        symbolState.Low < _bollingerBands.MiddleBand(symbolState.Symbol) && symbolState.High > _bollingerBands.MiddleBand(symbolState.Symbol);
    protected override bool CloseShortCondition(SymbolState symbolState) => 
        symbolState.Close > _bollingerBands.MiddleBand(symbolState.Symbol);


    public CustomBBB(IReadOnlyDictionary<string, string> rawParameters)
    {
        var parameters = rawParameters.ToDictionary(kv => kv.Key.ToLower(), kv => kv.Value);

        var middlePeriod = int.Parse(parameters["middleperiod"]);
        var sidePeriod = int.Parse(parameters["sideperiod"]);
        var stdDevMultiplier = decimal.Parse(parameters["stdm"]);
        var source = parameters["source"].ToLower();
        _bollingerBands = new CustomBollingerBands(middlePeriod, sidePeriod, stdDevMultiplier, source);
    }
}