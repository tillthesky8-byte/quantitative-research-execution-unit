using Domain.Enums;
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
            INSERT INTO runs 
            (run_id, ran_at, strategy_name, strategy_hash, dataset_hash, config_json)
            VALUES (?, ?, ?, ?, ?, ?)
        ";

        using var command = connection.CreateCommand();
        command.CommandText = query;
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.RunId });
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.RanAt });
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.Simulator?.Strategy?.Type.ToString() ?? "unknown" });
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.StrategyHash });
        command.Parameters.Add(new DuckDBParameter { Value = runConfig.DatasetHash });
        command.Parameters.Add(new DuckDBParameter { Value = configJson });

        await command.ExecuteNonQueryAsync();
    }

    private async Task WriteEquityCurve(DuckDBConnection connection, Guid runId, IEnumerable<EquityPoint> equityCurve)
    {
        using var create = connection.CreateCommand();
        create.CommandText = @"
            CREATE TEMP TABLE temp_equity_curve (
                run_id UUID,
                timestamp BIGINT,
                equity DOUBLE,
                cash DOUBLE
            );
        ";
        await create.ExecuteNonQueryAsync();

        using (var appender = connection.CreateAppender("temp_equity_curve"))
        {
            foreach (var point in equityCurve)
            {
                var row = appender.CreateRow();
                row.AppendValue(runId);
                row.AppendValue(point.Time);
                row.AppendValue((double)point.Equity);
                row.AppendValue((double)point.Cash);

                row.EndRow();
            }
        }

        await ExportTempTableAsync(connection, "temp_equity_curve", $"data/runs/{runId}/equity.parquet");

    }

    private async Task WritePnlEvents(DuckDBConnection connection, Guid runId, IEnumerable<RealizedPnlEvent> pnlEvents)
    {
        const string createTempTable = @"
            CREATE TEMP TABLE temp_pnl (
                run_id UUID,
                timestamp BIGINT,
                symbol VARCHAR,
                quantity DOUBLE,
                pnl DOUBLE,
                commission DOUBLE,
                entry_price DOUBLE,
                exit_price DOUBLE
            );
        ";

        await CreateTempTableAsync(connection, createTempTable);

        using (var appender = connection.CreateAppender("temp_pnl"))
        {
            foreach (var pnlEvent in pnlEvents)
            {
                var row = appender.CreateRow();
                row.AppendValue(runId);
                row.AppendValue(pnlEvent.Timestamp);
                row.AppendValue(pnlEvent.Symbol);
                row.AppendValue((double)pnlEvent.Quantity);
                row.AppendValue((double)pnlEvent.RealizedPnl);
                row.AppendValue((double)pnlEvent.Commission);
                row.AppendValue((double)pnlEvent.EntryPrice);
                row.AppendValue((double)pnlEvent.ExitPrice);

                row.EndRow();
            }
        }

        await ExportTempTableAsync(connection, "temp_pnl", $"data/runs/{runId}/pnl.parquet");
    }

    private async Task WriteTrades(DuckDBConnection connection, Guid runId, IEnumerable<TradeRecord> trades)
    {
        const string createTempTable = @"
            CREATE TEMP TABLE temp_trade_events (
                run_id UUID,
                timestamp BIGINT,
                symbol VARCHAR,
                side VARCHAR,
                action VARCHAR,
                quantity DOUBLE,
                price DOUBLE,
                commission DOUBLE
            );
        ";

        await CreateTempTableAsync(connection, createTempTable);

        using (var appender = connection.CreateAppender("temp_trade_events"))
        {
            foreach (var trade in trades)
            {
                var row = appender.CreateRow();
                row.AppendValue(runId);
                row.AppendValue(trade.Time);
                row.AppendValue(trade.Symbol);
                row.AppendValue(trade.Side);
                row.AppendValue(trade.Action.ToString());
                row.AppendValue((double)trade.Quantity);
                row.AppendValue((double)trade.Price);
                row.AppendValue((double)trade.Commission);

                row.EndRow();
            }
        }

        await ExportTempTableAsync(connection, "temp_trade_events", $"data/runs/{runId}/trades.parquet");
    }

    private static async Task CreateTempTableAsync(DuckDBConnection connection, string commandText)
    {
        await using var command = connection.CreateCommand();
        command.CommandText = commandText;
        await command.ExecuteNonQueryAsync();
    }

    private static async Task ExportTempTableAsync(DuckDBConnection connection, string tableName, string outputPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        await using var export = connection.CreateCommand();
        export.CommandText = $@"
            COPY (
                SELECT * FROM {tableName}
            )
            TO ?
            (FORMAT PARQUET);
        ";
        export.Parameters.Add(new DuckDBParameter { Value = outputPath });
        await export.ExecuteNonQueryAsync();
    }
}