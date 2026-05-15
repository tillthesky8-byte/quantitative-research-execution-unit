using Domain.Interfaces;

namespace Domain.Models;

public record OhlcvEvent : IMarketEvent
{
    public long Timestamp { get; init; }
    public required string Symbol { get; init; }
    public decimal Open { get; init; }
    public decimal High { get; init; }
    public decimal Low { get; init; }
    public decimal Close { get; init; }
    public decimal Volume { get; init; }
}