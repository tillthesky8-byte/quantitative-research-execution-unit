using System.Text.Json;
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

    public async Task<IEnumerable<Run>> GetRunsAsync()
    {
        var query = "SELECT run_id, ran_at, strategy_name, config_json, metrics_json FROM runs_data";
        using var connection = await _dbConnectionFactory.CreateConnectionAsync();
        await connection.OpenAsync();

        using var command = connection.CreateCommand();
        command.CommandText = query;

        using var reader = await command.ExecuteReaderAsync();

        var runs = new List<Run>();
        while (await reader.ReadAsync())
        {
            var configString = reader.GetString(3);
            var configJson = JsonSerializer.Deserialize<ConfigJson>(configString);
            var run = new Run
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
}