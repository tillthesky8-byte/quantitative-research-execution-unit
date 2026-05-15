using Domain.Interfaces;

namespace Domain.Models;

public record FactorEvent : IMarketEvent
{
    public long Timestamp { get; init; }
    public required string Symbol { get; init; }
    public required string Factor { get; init; }
    public decimal Value { get; init; }
}