using Studio.Web.Interfaces;
using Studio.Web.Models;
using DuckDB.NET.Data;
namespace Studio.Web.Repositories;

public class SeriesRepository : ISeriesRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<SeriesRepository> _logger;
    public SeriesRepository(IDbConnectionFactory dbConnectionFactory, ILogger<SeriesRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<SeriesBundle> GetSeriesBundleAsync(Guid runId, string symbol, DateTime from, DateTime to)
    {
        var ohlcTask = GetOhlcAsync(symbol, from, to);
        var equityCurveTask = GetEquityCurveAsync(runId, from, to);
        await Task.WhenAll(ohlcTask, equityCurveTask);
        return new SeriesBundle(ohlcTask.Result, equityCurveTask.Result);
    }
    public async Task<List<Ohlc>> GetOhlcAsync(string symbol, DateTime from, DateTime to)
    {
        var ohlcList = new List<Ohlc>();


        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();

        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT timestamp, open, high, low, close
            FROM ohlcv_data
            WHERE symbol = ? AND timestamp >= ? AND timestamp <= ?
            ORDER BY timestamp ASC
        ";

        command.Parameters.Add(new DuckDBParameter { Value = symbol });
        command.Parameters.Add(new DuckDBParameter { Value = ToUnixTimeSeconds(from) });
        command.Parameters.Add(new DuckDBParameter { Value = ToUnixTimeSeconds(to) });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var ohlc = new Ohlc(
                Time: reader.GetInt64(0),
                Open: reader.GetDouble(1),
                High: reader.GetDouble(2),
                Low: reader.GetDouble(3),
                Close: reader.GetDouble(4)
            );
            ohlcList.Add(ohlc);
        }
        return ohlcList;
    }

    public async Task<List<EquityPoint>> GetEquityCurveAsync(Guid runId, DateTime from, DateTime to)
    {
        var equityPoints = new List<EquityPoint>();
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT timestamp, equity
            FROM equity_curve
            WHERE run_id = ? AND timestamp >= ? AND timestamp <= ?
            ORDER BY timestamp ASC
        ";

        command.Parameters.Add(new DuckDBParameter { Value = runId });
        command.Parameters.Add(new DuckDBParameter { Value = ToUnixTimeSeconds(from) });
        command.Parameters.Add(new DuckDBParameter { Value = ToUnixTimeSeconds(to) });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var equityPoint = new EquityPoint(
                Time: reader.GetInt64(0),
                Value: reader.GetDouble(1)
            );
            equityPoints.Add(equityPoint);
        }
        return equityPoints;
    }

    public async Task<List<Trade>> GetTradesAsync(Guid runId, DateTime from, DateTime to, int page = 1, int pageSize = 100)
    {
        var trades = new List<Trade>();
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT timestamp, symbol, side, action, price, quantity, commission
            FROM trade_events
            WHERE run_id = ? AND timestamp >= ? AND timestamp <= ?
            ORDER BY timestamp ASC
            LIMIT ? OFFSET ?
        ";

        command.Parameters.Add(new DuckDBParameter { Value = runId });
        command.Parameters.Add(new DuckDBParameter { Value = ToUnixTimeSeconds(from) });
        command.Parameters.Add(new DuckDBParameter { Value = ToUnixTimeSeconds(to) });
        command.Parameters.Add(new DuckDBParameter { Value = pageSize });
        command.Parameters.Add(new DuckDBParameter { Value = (page - 1) * pageSize });

        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())        {
            var trade = new Trade(
                Time: reader.GetInt64(0),
                Symbol: reader.GetString(1),
                Side: reader.GetString(2),
                Action: reader.GetString(3),
                Price: reader.GetDouble(4),
                Quantity: reader.GetDouble(5),
                Commission: reader.GetDouble(6)
            );
            trades.Add(trade);
        }
        return trades;
    }

    private long ToUnixTimeSeconds(DateTime dateTime) => new DateTimeOffset(dateTime).ToUnixTimeSeconds();

}