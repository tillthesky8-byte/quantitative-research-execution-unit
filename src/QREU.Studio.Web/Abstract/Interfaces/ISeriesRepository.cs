using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface ISeriesRepository
{

    Task<List<Ohlc>> GetOhlcAsync(string symbol, int timeframeInSeconds, long from, long to);
    Task<List<EquityPoint>> GetEquityCurveAsync(Guid runId, int timeframeInSeconds, long from, long to);    
    Task<List<Trade>> GetTradesAsync(Guid runId, DateTime from, DateTime to, int page = 1, int pageSize = 100);
}