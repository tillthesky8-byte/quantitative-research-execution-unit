using Domain.Interfaces;
using Domain.Models;

namespace Core.States;

public sealed class MarketState
{
    public long Timestamp { get; set; }
    public Dictionary<string, SymbolState> Symbols { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public decimal GetPriceForSymbol(string symbol)
    {
        if (Symbols.TryGetValue(symbol, out var symbolState))
        {
            return symbolState.Close;
        }
        throw new KeyNotFoundException($"Symbol '{symbol}' not found in market state.");
    }

    public SymbolState GetOhlcvForSymbol(string symbol)
    {
        if (Symbols.TryGetValue(symbol, out var symbolState))
        {
            return symbolState;
        }
        throw new KeyNotFoundException($"Symbol '{symbol}' not found in market state.");
    }

    public void UpdateState(IMarketEvent marketEvent)
    {
        if (!Symbols.TryGetValue(marketEvent.Symbol, out var symbolState))
        {
            symbolState = new SymbolState { Symbol = marketEvent.Symbol };
            Symbols[marketEvent.Symbol] = symbolState;
        }

        switch (marketEvent)
        {
            case OhlcvEvent ohlcvEvent:
                symbolState.UpdateOhlcv(ohlcvEvent);
                break;
            case FactorEvent factorEvent:
                symbolState.UpdateFactor(factorEvent);
                break;
            default:
                throw new ArgumentException($"Unsupported market event type: {marketEvent.GetType().Name}");
        }

        Timestamp = marketEvent.Timestamp;
    }
    
}
