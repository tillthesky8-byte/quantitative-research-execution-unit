using Domain.Models;
using DuckDB.NET.Data;
using System.Text.Json;

namespace Writer;

public class WriteManager
{
    private readonly string _connectionString;
    private readonly RunConfiguration _runConfig;

    public WriteManager(RunConfiguration runConfig, string connectionString)
    {
        _runConfig = runConfig;
        _connectionString = connectionString;
    }

    public async Task WriteDataAsync(SimulationResult simulationResult)
    {
        await using var connection = new DuckDBConnection(_connectionString);
        await connection.OpenAsync();

        await WriteRun(connection, _runConfig, simulationResult);

        if (_runConfig.IncludeEquityCurve)
            await WriteEquityCurve(connection, _runConfig.RunId, simulationResult.EquityCurve);
        
        if (_runConfig.IncludePnlEvents)
            await WritePnlEvents(connection, _runConfig.RunId, simulationResult.RealizedPnlEvents); 

        if (_runConfig.IncludeTrades)
            await WriteTrades(connection, _runConfig.RunId, simulationResult.TradeRecords);
    }

    private async Task WriteRun(DuckDBConnection connection, RunConfiguration runConfig, SimulationResult simulationResult)
    {
        var configJson = JsonSerializer.Serialize(runConfig);
        var metricsJson = JsonSerializer.Serialize(simulationResult.Metrics);

        const string query = @"
            INSERT INTO runs_data 
            (run_id, ran_at, strategy_name, strategy_hash, dataset_hash, config_json, metrics_json)
            VALUES (?, ?, ?, ?, ?, ?, ?)
        ";

        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.RunId });
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.RanAt });
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.Simulator?.Strategy?.Type.ToString() ?? "unknown" });
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.StrategyHash });
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.DatasetHash });
        command.Parameters.Add(new DuckDBParameter { Value = configJson });
        command.Parameters.Add(new DuckDBParameter { Value = metricsJson });

        await command.ExecuteNonQueryAsync();
    }

    private async Task WriteEquityCurve(DuckDBConnection connection, string runId, IEnumerable<EquityPoint> equityCurve)
    {
        const string query = @"
            INSERT INTO equity_curve
            (run_id, timestamp, equity)
            VALUES (?, ?, ?)
        ";

        foreach (var point in equityCurve)
        {
            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.Add(new DuckDBParameter { Value = runId });
            command.Parameters.Add(new DuckDBParameter { Value = point.Time });
            command.Parameters.Add(new DuckDBParameter { Value = Convert.ToDouble(point.Equity) });

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task WritePnlEvents(DuckDBConnection connection, string runId, IEnumerable<RealizedPnlEvent> pnlEvents)
    {
        const string query = @"
            INSERT INTO realized_pnl_events
            (run_id, timestamp, symbol, quantity_closed, entry_price, exit_price, pnl)
            VALUES (?, ?, ?, ?, ?, ?, ?)
        ";

        foreach (var pnlEvent in pnlEvents)
        {
            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.Add(new DuckDBParameter { Value = runId });
            command.Parameters.Add(new DuckDBParameter { Value = pnlEvent.Timestamp });
            command.Parameters.Add(new DuckDBParameter { Value = pnlEvent.Symbol });
            command.Parameters.Add(new DuckDBParameter { Value = Convert.ToDouble(pnlEvent.Quantity) });
            command.Parameters.Add(new DuckDBParameter { Value = Convert.ToDouble(pnlEvent.EntryPrice) });
            command.Parameters.Add(new DuckDBParameter { Value = Convert.ToDouble(pnlEvent.ExitPrice) });
            command.Parameters.Add(new DuckDBParameter { Value = Convert.ToDouble(pnlEvent.RealizedPnl) });

            await command.ExecuteNonQueryAsync();
        }
    }

    private async Task WriteTrades(DuckDBConnection connection, string runId, IEnumerable<TradeRecord> trades)
    {
        const string query = @"
            INSERT INTO trade_events
            (run_id, timestamp, symbol, action, side, quantity, price, commission)
            VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        ";

        foreach (var trade in trades)
        {
            using var command = connection.CreateCommand();
            command.CommandText = query;
            command.Parameters.Add(new DuckDBParameter { Value = runId });
            command.Parameters.Add(new DuckDBParameter { Value = trade.Time });
            command.Parameters.Add(new DuckDBParameter { Value = trade.Symbol });
            command.Parameters.Add(new DuckDBParameter { Value = trade.Action.ToString() });
            command.Parameters.Add(new DuckDBParameter { Value = ((Domain.Enums.OrderSide)trade.Side).ToString() });
            command.Parameters.Add(new DuckDBParameter { Value = Convert.ToDouble(trade.Quantity) });
            command.Parameters.Add(new DuckDBParameter { Value = Convert.ToDouble(trade.Price) });
            command.Parameters.Add(new DuckDBParameter { Value = Convert.ToDouble(trade.Commission) });

            await command.ExecuteNonQueryAsync();
        }
    }
}