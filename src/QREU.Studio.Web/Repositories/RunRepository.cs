using System.Text.Json;
using DuckDB.NET.Data;
using Studio.Web.Interfaces;
using Studio.Web.Models;

namespace Studio.Web.Repositories;

public class RunRepository : IRunRepository
{
    private readonly ILogger<RunRepository> _logger;
    private readonly IDbConnectionFactory _dbConnectionFactory;

    public RunRepository(ILogger<RunRepository> logger, IDbConnectionFactory dbConnectionFactory)
    {
        _logger = logger;
        _dbConnectionFactory = dbConnectionFactory;
    }

    public async Task<IEnumerable<RawRun>> GetRunsAsync()
    {
        var query = "SELECT run_id, ran_at, strategy_name, config_json, metrics_json FROM runs_data";
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = query;

        using var reader = await command.ExecuteReaderAsync();

        var runs = new List<RawRun>();
        while (await reader.ReadAsync())
        {
            var configString = reader.GetString(3);
            var configJson = JsonSerializer.Deserialize<ConfigJson>(configString);
            var run = new RawRun
            (
                Id           : reader.GetGuid(0),
                RanAt        : reader.GetDateTime(1),
                StrategyName : reader.GetString(2),
                ConfigJson   : configJson ?? throw new InvalidOperationException("Failed to deserialize ConfigJson"),
                MetricsJson  : reader.GetString(4)
            );
            runs.Add(run);
        }
        return runs;
    }

    public async Task<RawRun> GetRunAsync(Guid runId)
    {
        var query = "SELECT run_id, ran_at, strategy_name, config_json, metrics_json FROM runs_data WHERE run_id = ?";
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();
        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.Add(new DuckDBParameter { Value = runId });
        using var reader = await command.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            var configString = reader.GetString(3);
            var configJson = JsonSerializer.Deserialize<ConfigJson>(configString);
            var run = new RawRun
            (
                Id           : reader.GetGuid(0),
                RanAt        : reader.GetDateTime(1),
                StrategyName : reader.GetString(2),
                ConfigJson   : configJson ?? throw new InvalidOperationException("Failed to deserialize ConfigJson"),
                MetricsJson  : reader.GetString(4)
            );
            return run;
        }
        else
        {
            return null!;
        }
    }
}