using DuckDB.NET.Data;
using System.Globalization;
using Ingestion.Interfaces;
namespace Ingestion.Ingestors;

public class OhlcvIngestor : IOhlcvIngestor
{
    private readonly string _connectionString;
    private readonly string _csvDirectory;
    private readonly string _parquetDirectory;

    public OhlcvIngestor(string connectionString, string csvDirectory, string parquetDirectory)
    {
        _connectionString = connectionString;
        _csvDirectory     = csvDirectory;
        _parquetDirectory = parquetDirectory;
    }

    public async Task Ingest(string symbol)
    {
        var csvFilePath = Path.Combine(_csvDirectory, $"{symbol}.csv");;

        if (!await TryValidateCsvAsync(csvFilePath, symbol))
            return;

        await WriteParquetAsync(csvFilePath, symbol);
    }

    private async Task<bool> TryValidateCsvAsync(string csvFilePath, string symbol)
    {
        if (!File.Exists(csvFilePath))
        {
            Console.WriteLine($"CSV file for symbol {symbol} not found at path: {csvFilePath}");
            return false;
        }

        await ValidateCsvStructureAsync(csvFilePath);
        await ValidateMonotonicTimestampsAsync(csvFilePath);
        return true;
    }

    private async Task ValidateCsvStructureAsync(string csvFilePath)
    {
        using var reader = new StreamReader(csvFilePath);

        var headerLine = await reader.ReadLineAsync();

        if (headerLine == null)
            throw new InvalidDataException("CSV file is empty.");

        var headers = headerLine.Split(',');

        ValidateOhlcvStructure(headers);
    }

    private async Task ValidateMonotonicTimestampsAsync(string csvFilePath)
    {
        using var reader = new StreamReader(csvFilePath);

        await reader.ReadLineAsync();

        string? line;
        long? lastTimestamp = null;

        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(',');
            if (parts.Length < 6)
                continue;

            if (!long.TryParse(parts[0], NumberStyles.Any, CultureInfo.InvariantCulture, out var ts))
                throw new InvalidDataException($"Invalid timestamp: {parts[0]}");

            if (lastTimestamp != null && ts <= lastTimestamp)
                throw new InvalidDataException("Non-monotonic timestamp detected in CSV.");

            lastTimestamp = ts;
        }
    }

    private async Task ExecuteCsvInsertAsync(DuckDBConnection connection, string csvFilePath, string symbol)
    {
        await using var cmd = connection.CreateCommand();

        cmd.CommandText = $@"
            INSERT INTO ohlcv
            SELECT
                '{symbol}' AS symbol,
                CAST(column0 AS BIGINT) AS timestamp,
                CAST(column1 AS DOUBLE) AS open,
                CAST(column2 AS DOUBLE) AS high,
                CAST(column3 AS DOUBLE) AS low,
                CAST(column4 AS DOUBLE) AS close,
                CAST(column5 AS DOUBLE) AS volume
            FROM read_csv_auto('{csvFilePath}', HEADER=true);
        ";

        await cmd.ExecuteNonQueryAsync();
    }
    private async Task WriteParquetAsync(string csvFilePath, string symbol)
    {
        var outputPath = Path.Combine(_parquetDirectory,"ohlcv", $"{symbol}.parquet");
        Directory.CreateDirectory(_parquetDirectory);

        await using var connection = new DuckDBConnection(_connectionString);
        await connection.OpenAsync();

        await using var cmd = connection.CreateCommand();
        cmd.CommandText = $@"
        COPY (
            SELECT
                '{symbol}' AS symbol,
                t.timestamp,
                t.open,
                t.high,
                t.low,
                t.close,
                t.volume
            FROM read_csv_auto(
                '{csvFilePath}',
                HEADER=true,
                columns={{
                    'timestamp':'BIGINT',
                    'open':'DOUBLE',
                    'high':'DOUBLE',
                    'low':'DOUBLE',
                    'close':'DOUBLE',
                    'volume':'DOUBLE'
                }}
            ) AS t
        )
        TO '{outputPath}'
        (FORMAT PARQUET);
        ";

        await cmd.ExecuteNonQueryAsync();
    }

    private void ValidateOhlcvStructure(string[] headers)
    {
        var expectedHeaders = new[] { "timestamp", "open", "high", "low", "close", "volume" };

        if (!expectedHeaders.SequenceEqual(headers, StringComparer.OrdinalIgnoreCase))
        {
            throw new InvalidDataException(
                $"CSV file has invalid structure. Expected: {string.Join(", ", expectedHeaders)}");
        }
    }
}