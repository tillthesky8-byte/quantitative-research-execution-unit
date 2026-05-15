namespace Domain.Models;

public sealed class Position
{
    public required string Symbol { get; init; }
    public required decimal Quantity { get; set; }
    public required decimal AverageEntryPrice { get; set; }

    public bool IsLong => Quantity > 0;
    public bool IsShort => Quantity < 0;
    public bool IsFlat => Quantity == 0;


    public decimal GetUnrealizedPnL(decimal currentPrice) => (currentPrice - AverageEntryPrice) * Quantity;

    public decimal GetMarketValue(decimal currentPrice) => currentPrice * Quantity;


}