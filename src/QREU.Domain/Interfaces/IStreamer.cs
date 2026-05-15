namespace Domain.Interfaces;

public interface IStreamer : IAsyncDisposable
{
    IAsyncEnumerable<IMarketEvent> StreamAsync();
}
