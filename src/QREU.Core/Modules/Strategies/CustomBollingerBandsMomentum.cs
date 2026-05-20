using Core.Interfaces;
using Domain.Interfaces;
using Domain.Models;
using Modules.Families;
using Modules.Indicators;

namespace Modules.Strategies;

public sealed class CustomBollingerBandsMomentum : ExitEntryStrategy, IStrategy
{
    private readonly CustomBollingerBands _bollingerBands;

    protected override IIndicator[] Indicators => [_bollingerBands];

    protected override bool OpenLongCondition(SymbolState symbolState) =>
        _bollingerBands.IsReady && symbolState.Close > _bollingerBands.UpperBand(symbolState.Symbol);

    protected override bool OpenShortCondition(SymbolState symbolState) =>
        _bollingerBands.IsReady && symbolState.Close < _bollingerBands.LowerBand(symbolState.Symbol);

    protected override bool CloseLongCondition(SymbolState symbolState) =>
        _bollingerBands.IsReady && symbolState.Close < _bollingerBands.MiddleBand(symbolState.Symbol);

    protected override bool CloseShortCondition(SymbolState symbolState) =>
        _bollingerBands.IsReady && symbolState.Close > _bollingerBands.MiddleBand(symbolState.Symbol);

    public CustomBollingerBandsMomentum(IReadOnlyDictionary<string, string> rawParameters)
    {
        var parameters = rawParameters.ToDictionary(kv => Normalize(kv.Key), kv => kv.Value);

        var middlePeriod = int.Parse(GetRequired(parameters, "middlebandperiod", "middleperiod"));
        var sidePeriod = int.Parse(GetRequired(parameters, "sidebandsperiod", "sideperiod"));
        var stdDevMultiplier = decimal.Parse(GetRequired(parameters, "standarddeviationmultiplier", "stddevmultiplier", "stdm"));
        var source = GetRequired(parameters, "source", "computationsource").ToLowerInvariant();

        _bollingerBands = new CustomBollingerBands(middlePeriod, sidePeriod, stdDevMultiplier, source);
    }

    private static string Normalize(string key) => key.Replace("_", string.Empty).Replace("-", string.Empty).Replace(" ", string.Empty).ToLowerInvariant();

    private static string GetRequired(IReadOnlyDictionary<string, string> parameters, params string[] aliases)
    {
        foreach (var alias in aliases)
        {
            if (parameters.TryGetValue(alias, out var value) && !string.IsNullOrWhiteSpace(value))
                return value;
        }

        throw new ArgumentException($"Missing required parameter. Any of the following keys is accepted: {string.Join(", ", aliases)}");
    }
}
