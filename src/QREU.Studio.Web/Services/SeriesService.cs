using Studio.Web.Interfaces;
using Studio.Web.Models;

namespace Studio.Web.Services;

public class SeriesService : ISeriesService
{
    private readonly ILogger<SeriesService> _logger;

    private readonly ISeriesRepository _seriesRepository;
    private readonly ISeriesChunkRepository _seriesChunkRepository;

    public SeriesService(ILogger<SeriesService> logger, ISeriesRepository seriesRepository, ISeriesChunkRepository seriesChunkRepository)
    {
        _logger = logger;
        _seriesRepository = seriesRepository;
        _seriesChunkRepository = seriesChunkRepository;
    }
    public Task<SeriesBundle> GetSeriesBundleAsync(Guid runId, string symbol, string timeframe, DateTime from, DateTime to)
    {
        var unixFrom = ToUnixTimeSeconds(from);
        var unixTo = ToUnixTimeSeconds(to);
        var timeframeSeconds = SecondsTimeframeMapper(timeframe);

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
    
    public Task<SeriesBundle> GetForwardSeriesChunkBundleAsync(Guid runId, string symbol, string timeframe, long to, int chunkSize)
    {
        var timeframeMilliseconds = MillisecondsTimeframeMapper(timeframe);
        var ohlcTask = _seriesChunkRepository.GetForwardOhlcvChunkAsync(symbol, timeframeMilliseconds, to, chunkSize);
        var equityTask = _seriesChunkRepository.GetForwardEquityChunkAsync(runId, timeframeMilliseconds, to, chunkSize);

        return Task.WhenAll(ohlcTask, equityTask)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Failed to get forward series chunk bundle for run {RunId}, symbol {Symbol}, timeframe {Timeframe}", runId, symbol, timeframe);
                    throw t.Exception!;
                }
                return new SeriesBundle(ohlcTask.Result, equityTask.Result);
            });
    }

    public Task<SeriesBundle> GetBackwardSeriesChunkBundleAsync(Guid runId, string symbol, string timeframe, long from, int chunkSize)
    {
        var timeframeMilliseconds = MillisecondsTimeframeMapper(timeframe);
        var ohlcTask = _seriesChunkRepository.GetBackwardOhlcvChunkAsync(symbol, timeframeMilliseconds, from, chunkSize);
        var equityTask = _seriesChunkRepository.GetBackwardEquityChunkAsync(runId, timeframeMilliseconds, from, chunkSize);

        return Task.WhenAll(ohlcTask, equityTask)
            .ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    _logger.LogError(t.Exception, "Failed to get backward series chunk bundle for run {RunId}, symbol {Symbol}, timeframe {Timeframe}", runId, symbol, timeframe);
                    throw t.Exception!;
                }
                return new SeriesBundle(ohlcTask.Result, equityTask.Result);
            });
    }

    private long ToUnixTimeSeconds(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeSeconds();
    }
    private long ToUnixTimeMilliseconds(DateTime dateTime)
    {
        return new DateTimeOffset(dateTime).ToUnixTimeMilliseconds();
    }
    private int SecondsTimeframeMapper(string timeframe)
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

    private int MillisecondsTimeframeMapper(string timeframe)
    {
        return timeframe switch
        {
            "1m" => 60_000,
            "5m" => 300_000,
            "15m" => 900_000,
            "30m" => 1_800_000,
            "1h" => 3_600_000,
            "4h" => 14_400_000,
            "1d" => 86_400_000,
            _ => throw new ArgumentException($"Unsupported timeframe: {timeframe}")
        };
    }
}