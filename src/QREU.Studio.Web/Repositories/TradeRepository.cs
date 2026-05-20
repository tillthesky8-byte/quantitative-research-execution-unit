using Studio.Web.Interfaces;
using Studio.Web.Models;
using DuckDB.NET.Data;
namespace Studio.Web.Repositories;

public class TradeRepository : ITradeRepository
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<TradeRepository> _logger;

    public TradeRepository(IDbConnectionFactory dbConnectionFactory, ILogger<TradeRepository> logger)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
    }   

    public async Task<List<Trade>> GetTradesAsync(Guid runId, int page = 1, int pageSize = 100)
    {
        var trades = new List<Trade>();
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        var command = connection.CreateCommand();
        var parquetPath = $"../../data/runs/{runId}/trades.parquet";
        command.CommandText = @"
            SELECT timestamp, symbol, side, action, price, quantity, commission
            FROM read_parquet(?)
            ORDER BY timestamp DESC
            LIMIT ? OFFSET ?";
        command.Parameters.Add(new DuckDBParameter { Value = parquetPath });
        command.Parameters.Add(new DuckDBParameter { Value = pageSize });
        command.Parameters.Add(new DuckDBParameter { Value = (page - 1) * pageSize });
        using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
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
}