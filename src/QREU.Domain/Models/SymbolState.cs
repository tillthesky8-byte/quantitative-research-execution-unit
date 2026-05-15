using Domain.Models;

namespace Domain.Models;
public sealed class SymbolState
{
    public long LastUpdated { get; set; }
    public required string Symbol { get; init; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }

    public Dictionary<string, decimal> Factors { get; } =
        new(StringComparer.OrdinalIgnoreCase);
    
    public decimal this[string columnName]
    {
        get
        {
            return columnName.ToLowerInvariant() switch
            {
                "open" => Open,
                "high" => High,
                "low" => Low,
                "close" => Close,
                "volume" => Volume,
                _ => Factors.TryGetValue(columnName, out var value) ? value
                    : throw new ArgumentException($"Column '{columnName}' not found in SymbolState.")
            };
        }
    }

    public void UpdateOhlcv(OhlcvEvent ohlcvEvent)
    {
        Open   = ohlcvEvent.Open;
        High   = ohlcvEvent.High;
        Low    = ohlcvEvent.Low;
        Close  = ohlcvEvent.Close;
        Volume = ohlcvEvent.Volume;
        LastUpdated = ohlcvEvent.Timestamp;
    }

    public void UpdateFactor(FactorEvent factorEvent)
    {
        Factors[factorEvent.Factor] = factorEvent.Value;
        LastUpdated = factorEvent.Timestamp;
    }
}