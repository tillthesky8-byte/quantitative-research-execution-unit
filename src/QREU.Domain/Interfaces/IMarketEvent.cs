namespace Domain.Interfaces;

public interface IMarketEvent
{
    long Timestamp { get; }
    string Symbol { get; }
}
