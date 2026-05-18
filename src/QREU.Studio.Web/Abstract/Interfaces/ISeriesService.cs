using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface ISeriesService
{
    Task<SeriesBundle> GetSeriesBundleAsync(Guid runId, string symbol, string timeframe, DateTime from, DateTime to);
}