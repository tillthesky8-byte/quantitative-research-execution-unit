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

    public async Task<List<Ohlc>> GetOhlcAsync(string symbol, int timeframeInSeconds, long from, long to)
    {
        var ohlcList = new List<Ohlc>();

        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                symbol,
                floor(timestamp / ?) * ? AS time,
                first(open) AS open,
                max(high) AS high,
                min(low) AS low,
                last(close) AS close
            FROM ohlcv_data
            WHERE symbol = ? AND timestamp >= ? AND timestamp <= ?
            GROUP BY symbol, time
            ORDER BY time ASC ";
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInSeconds });
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInSeconds });
        command.Parameters.Add(new DuckDBParameter { Value = symbol });
        command.Parameters.Add(new DuckDBParameter { Value = from });
        command.Parameters.Add(new DuckDBParameter { Value = to });
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())        
        {
            var ohlc = new Ohlc(
                Time: reader.GetInt64(1),
                Open: reader.GetDouble(2),
                High: reader.GetDouble(3),
                Low: reader.GetDouble(4),
                Close: reader.GetDouble(5)
            );
            ohlcList.Add(ohlc);
        }
        return ohlcList;
    }

    public async Task<List<EquityPoint>> GetEquityCurveAsync(Guid runId, int timeframeInSeconds, long from, long to)
    {
        var equityPoints = new List<EquityPoint>();
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                floor(timestamp / ?) * ? AS time,
                last(equity) AS value
            FROM equity_curve
            WHERE run_id = ? AND timestamp >= ? AND timestamp <= ?
            GROUP BY time
            ORDER BY time ASC ";
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInSeconds });
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInSeconds });
        command.Parameters.Add(new DuckDBParameter { Value = runId });
        command.Parameters.Add(new DuckDBParameter { Value = from });
        command.Parameters.Add(new DuckDBParameter { Value = to });
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