using Studio.Web.Interfaces;
using Studio.Web.Models;

namespace Studio.Web.Services;

public class SeriesService : ISeriesService
{
    private readonly ILogger<SeriesService> _logger;

    private readonly ISeriesRepository _seriesRepository;

    public SeriesService(ILogger<SeriesService> logger, ISeriesRepository seriesRepository)
    {
        _logger = logger;
        _seriesRepository = seriesRepository;
    }
    public Task<SeriesBundle> GetSeriesBundleAsync(Guid runId, string symbol, string timeframe, DateTime from, DateTime to)
    {
        var unixFrom = ToUnixTimeSeconds(from);
        var unixTo = ToUnixTimeSeconds(to);
        var timeframeSeconds = TimeframeMapper(timeframe);

        var ohlcTask = _seriesRepository.GetOhlcAsync(symbol, timeframeSeconds, unixFrom, unixTo);
        var equityTask = _seriesRepository.GetEquityCurveAsync(runId, timeframeSeconds, unixFrom, unixTo);

        return Task.WhenAll(ohlcTask, equityTask)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Failed to get series bundle for run {RunId}, symbol {Symbol}, timeframe {Timeframe}", runId, symbol, timeframe);
                    throw t.Exception!;
                }
                return new SeriesBundle(ohlcTask.Result, equityTask.Result);
            });
    }
    private long ToUnixTimeSeconds(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }
    private int TimeframeMapper(string timeframe)
    {
        return timeframe switch
        {
            "1m" => 60,
            "5m" => 300,
            "15m" => 900,
            "30m" => 1800,
            "1h" => 3600,
            "4h" => 14400,
            "1d" => 86400,
            _ => throw new ArgumentException($"Unsupported timeframe: {timeframe}")
        };
    }
}