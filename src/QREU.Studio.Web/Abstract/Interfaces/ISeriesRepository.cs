using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface ISeriesRepository
{
    Task<List<Ohlc>> GetOhlcAsync(string symbol, DateTime from, DateTime to);
    Task<List<EquityPoint>> GetEquityCurveAsync(Guid runId, DateTime from, DateTime to);
    Task<List<Trade>> GetTradesAsync(Guid runId, DateTime from, DateTime to, int page = 1, int pageSize = 100);
}