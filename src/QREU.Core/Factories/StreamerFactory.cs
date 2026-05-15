using Core.Stream;
using Domain.Definitions;
using Domain.Interfaces;
using Domain.Other;
using DuckDB.NET.Data;
using Microsoft.Extensions.Logging;

namespace Core.Factories;
public sealed class StreamerFactory
{
    private readonly ILogger<StreamerFactory>? _logger;
    private readonly ILoggerFactory _loggerFactory;

    private readonly string _connectionString;
    private readonly InstrumentDefinition[] _instruments;
    private readonly FactorDefinition[] _factors;
    public readonly DateTime StartDate;
    public readonly DateTime EndDate;

    public StreamerFactory(string connectionString, DatasetDefinition dataset, ILoggerFactory loggerFactory, ILogger<StreamerFactory>? logger)
    {
        _connectionString = connectionString;
        _instruments      = dataset.Instruments;
        _factors          = dataset.Factors;
        StartDate         = dataset.StartDate;
        EndDate           = dataset.EndDate;
        _logger           = logger;
        _loggerFactory    = loggerFactory;
    }

    public async Task<IStreamer> CreateStreamer()
    {
        var ohlcvQuery = BuildOhlcvQuery();
        var factorQuery = BuildFactorQuery();

        var commandOhlcv = new DuckDBCommand(ohlcvQuery);
        var commandFactor = new DuckDBCommand(factorQuery);

        AddOhlcvParameters(commandOhlcv);
        AddFactorParameters(commandFactor);

        var connection = new DuckDBConnection(_connectionString);

        try
        {
            await connection.OpenAsync();
        
            _logger?.LogInformation(LogMessages.ConnectionOpened, 
                _connectionString, 
                connection.Database, 
                connection.DataSource, 
                connection.State, 
                connection.ServerVersion);

            commandOhlcv.Connection = connection;
            commandFactor.Connection = connection;
        
            var readerOhlcv = await commandOhlcv.ExecuteReaderAsync();

            var readerFactor = await commandFactor.ExecuteReaderAsync();

            _logger?.LogInformation("Readers for Ohlcv and Factor queries created successfully.");

            return new Streamer(
                readerOhlcv,
                readerFactor,
                commandOhlcv,
                commandFactor,
                connection,
                _loggerFactory.CreateLogger<Streamer>());
        }
        catch
        {
            await connection.DisposeAsync();
            commandOhlcv.Dispose();
            commandFactor.Dispose();
            throw;
        }

    }
    
    private string BuildOhlcvQuery()
    {
        var placeholders = string.Join(", ", _instruments.Select(_ => "?"));
        return $@"
        SELECT
            timestamp,
            symbol,
            open,
            high,
            low,
            close,
            volume
        FROM ohlcv_data
        WHERE symbol IN ({placeholders})
          AND timestamp >= ?
          AND timestamp <= ?
        ORDER BY timestamp ASC
        ";
    }
    private string BuildFactorQuery()
    {
        var clauses = string.Join(" OR ", _factors.Select(f => "(factor = ? AND symbol = ?)"));
        return $@"
        SELECT
            timestamp,
            symbol,
            factor,
            value
        FROM factor_data
        WHERE ({clauses})
          AND timestamp >= ?
          AND timestamp <= ?
        ORDER BY timestamp ASC
        ";
    }
    private void AddOhlcvParameters(DuckDBCommand command)
    {
        foreach (var instrument in _instruments)
        {
            command.Parameters.Add(new DuckDBParameter { Value = instrument.Symbol });
            _logger?.LogDebug("Added Ohlcv parameter for instrument: {Symbol}", instrument.Symbol);
        }
        command.Parameters.Add(new DuckDBParameter { Value = ConvertToUnixTimestamp(StartDate) });
        _logger?.LogDebug("Added Ohlcv parameter for StartDate in seconds: {StartDate} -> {StartDateSeconds}", StartDate, ConvertToUnixTimestamp(StartDate));

        command.Parameters.Add(new DuckDBParameter { Value = ConvertToUnixTimestamp(EndDate)   });
        _logger?.LogDebug("Added Ohlcv parameter for EndDate in seconds: {EndDate} -> {EndDateSeconds}", EndDate, ConvertToUnixTimestamp(EndDate));
    }
    private void AddFactorParameters(DuckDBCommand command)
    {
        foreach (var factor in _factors)
        {
            command.Parameters.Add(new DuckDBParameter { Value = factor.Name });
            _logger?.LogDebug("Added Factor parameter for factor: {Name}", factor.Name);
            command.Parameters.Add(new DuckDBParameter { Value = factor.Symbol });
            _logger?.LogDebug("Added Factor parameter for symbol: {Symbol}", factor.Symbol);
        }
        _logger?.LogDebug("Added Factor parameter for StartDate in seconds: {StartDate} -> {StartDateSeconds}", StartDate, ConvertToUnixTimestamp(StartDate));
        command.Parameters.Add(new DuckDBParameter { Value = ConvertToUnixTimestamp(StartDate) });
        
        _logger?.LogDebug("Added Factor parameter for EndDate in seconds: {EndDate} -> {EndDateSeconds}", EndDate, ConvertToUnixTimestamp(EndDate));
        command.Parameters.Add(new DuckDBParameter { Value = ConvertToUnixTimestamp(EndDate) });
    }
    private long ConvertToUnixTimestamp(DateTime date) => new DateTimeOffset(date).ToUnixTimeSeconds();

}