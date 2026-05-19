using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface ISeriesService
{
    Task<SeriesBundle> GetSeriesBundleAsync(Guid runId, string symbol, string timeframe, DateTime from, DateTime to);
    Task<SeriesBundle> GetForwardSeriesChunkBundleAsync(Guid runId, string symbol, string timeframe, long to, int chunkSize);
    Task<SeriesBundle> GetBackwardSeriesChunkBundleAsync(Guid runId, string symbol, string timeframe, long from, int chunkSize);
}