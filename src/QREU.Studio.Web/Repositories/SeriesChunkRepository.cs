using System.Data;
using DuckDB.NET.Data;
using Studio.Web.Interfaces;
using Studio.Web.Models;

namespace Studio.Web.Repositories;

public class SeriesChunkRepository : ISeriesChunkRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<SeriesChunkRepository> _logger;  
    public SeriesChunkRepository(IDbConnectionFactory dbConnectionFactory, ILogger<SeriesChunkRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }

    public async Task<List<Ohlc>> GetBackwardOhlcvChunkAsync(string symbol, int timeframeInMilliseconds, long from, int chunkSize)
    {
        var ohlcList = new List<Ohlc>();

        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                symbol,
                floor(timestamp / ?) * ? AS time,
                arg_min(open, timestamp) AS open,
                max(high) AS high,
                min(low) AS low,
                arg_max(close, timestamp) AS close
            FROM read_parquet('../../data/parquet/ohlcv/*.parquet')
            WHERE symbol = ? AND timestamp <= ?
            GROUP BY symbol, time
            ORDER BY time DESC
            LIMIT ?";
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInMilliseconds });
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInMilliseconds });
        command.Parameters.Add(new DuckDBParameter { Value = symbol });
        command.Parameters.Add(new DuckDBParameter { Value = from });
        command.Parameters.Add(new DuckDBParameter { Value = chunkSize });
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

    public async Task<List<EquityPoint>> GetBackwardEquityChunkAsync(Guid runId, int timeframeInMilliseconds, long from, int chunkSize)
    {
        var equityPoints = new List<EquityPoint>();
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        var parquetPath = $"../../data/runs/{runId}/equity.parquet";
        command.CommandText = @"
            SELECT
                floor(timestamp / ?) * ? AS time,
                arg_max(equity, timestamp) AS equity
            FROM read_parquet(?)
            WHERE run_id = ? AND timestamp <= ?
            GROUP BY time
            ORDER BY time DESC
            LIMIT ?";
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInMilliseconds });
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInMilliseconds });
        command.Parameters.Add(new DuckDBParameter { Value = parquetPath });
        command.Parameters.Add(new DuckDBParameter { Value = runId.ToString() });
        command.Parameters.Add(new DuckDBParameter { Value = from });
        command.Parameters.Add(new DuckDBParameter { Value = chunkSize });
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var point = new EquityPoint(
                Time: reader.GetInt64(0),
                Value: reader.GetDouble(1)
            );
            equityPoints.Add(point);
        }
        return equityPoints;
    }

    public async Task<List<Ohlc>> GetForwardOhlcvChunkAsync(string symbol, int timeframeInMilliseconds, long to, int chunkSize)
    {
        var ohlcList = new List<Ohlc>();
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT
                symbol,
                floor(timestamp / ?) * ? AS time,
                arg_min(open, timestamp) AS open,
                max(high) AS high,
                min(low) AS low,
                arg_max(close, timestamp) AS close
            FROM read_parquet('../../data/parquet/ohlcv/*.parquet')
            WHERE symbol = ? AND timestamp >= ?
            GROUP BY symbol, time
            ORDER BY time ASC
            LIMIT ?";
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInMilliseconds });
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInMilliseconds });
        command.Parameters.Add(new DuckDBParameter { Value = symbol });
        command.Parameters.Add(new DuckDBParameter { Value = to });
        command.Parameters.Add(new DuckDBParameter { Value = chunkSize });
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

    public async Task<List<EquityPoint>> GetForwardEquityChunkAsync(Guid runId, int timeframeInMilliseconds, long to, int chunkSize)
    {
        var equityPoints = new List<EquityPoint>();
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        var parquetPath = $"../../data/runs/{runId}/equity.parquet";
        command.CommandText = @"
            SELECT
                floor(timestamp / ?) * ? AS time,
                arg_max(equity, timestamp) AS equity
            FROM read_parquet(?)
            WHERE run_id = ? AND timestamp >= ?
            GROUP BY time
            ORDER BY time ASC
            LIMIT ?";
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInMilliseconds });
        command.Parameters.Add(new DuckDBParameter { Value = timeframeInMilliseconds });
        command.Parameters.Add(new DuckDBParameter { Value = parquetPath });
        command.Parameters.Add(new DuckDBParameter { Value = runId.ToString() });
        command.Parameters.Add(new DuckDBParameter { Value = to });
        command.Parameters.Add(new DuckDBParameter { Value = chunkSize });
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var point = new EquityPoint(
                Time: reader.GetInt64(0),
                Value: reader.GetDouble(1)
            );
            equityPoints.Add(point);
        }
        return equityPoints;
    }

}