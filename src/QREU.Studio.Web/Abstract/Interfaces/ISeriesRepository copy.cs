using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface ISeriesChunkRepository
{
    Task<List<Ohlc>> GetBackwardOhlcvChunkAsync(string symbol, int timeframeInMilliseconds, long from, int chunkSize);
    Task<List<EquityPoint>> GetBackwardEquityChunkAsync(Guid runId, int timeframeInMilliseconds, long from, int chunkSize);

    Task<List<Ohlc>> GetForwardOhlcvChunkAsync(string symbol, int timeframeInMilliseconds, long to, int chunkSize);
    Task<List<EquityPoint>> GetForwardEquityChunkAsync(Guid runId, int timeframeInMilliseconds, long to, int chunkSize);
}
