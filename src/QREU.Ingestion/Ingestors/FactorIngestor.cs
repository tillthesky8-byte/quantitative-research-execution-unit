using DuckDB.NET.Data;
using System.Globalization;

namespace Ingestion.Ingestors;

/// <summary>
/// Ingests financial factor CSV files into parquet format.
/// Expected format: CSV with timestamp (unix ms) in first column, factor names in header, values in rows.
/// Transposes from wide (one row per timestamp, many columns) to long format (one row per timestamp-factor pair).
/// Output: {symbol}_factors.parquet with columns: timestamp, symbol, name, value
/// </summary>
public sealed class FactorIngestor
{
    private readonly string _connectionString;
    private readonly string _csvDirectory;
    private readonly string _parquetDirectory;

    public FactorIngestor(string connectionString, string csvDirectory, string parquetDirectory)
    {
        _connectionString = connectionString;
        _csvDirectory     = csvDirectory;
        _parquetDirectory = parquetDirectory;
    }

    public async Task Ingest(string symbol)
    {
        var csvFilePath = Path.Combine(_csvDirectory, $"{symbol}_factors.csv");

        if (!File.Exists(csvFilePath))
        {
            Console.WriteLine($"CSV file for symbol {symbol} not found at path: {csvFilePath}");
            return;
        }

        await ValidateCsvStructureAsync(csvFilePath);
        await WriteParquetAsync(csvFilePath, symbol);
    }

    private async Task ValidateCsvStructureAsync(string csvFilePath)
    {
        using var reader = new StreamReader(csvFilePath);
        var headerLine = await reader.ReadLineAsync();

        if (headerLine == null)
            throw new InvalidDataException("CSV file is empty.");

        var headers = CsvSplit(headerLine);
        if (headers.Count < 2)
            throw new InvalidDataException("CSV must have at least 2 columns (timestamp + at least one factor).");

        // First column should be 'timestamp'
        if (!headers[0].Equals("timestamp", StringComparison.OrdinalIgnoreCase))
            throw new InvalidDataException($"First column must be 'timestamp', got '{headers[0]}'.");

        // Validate at least one data row
        var firstDataLine = await reader.ReadLineAsync();
        if (firstDataLine == null)
            throw new InvalidDataException("CSV has no data rows.");

        var firstDataParts = CsvSplit(firstDataLine);
        if (!long.TryParse(firstDataParts[0], out _))
            throw new InvalidDataException($"First column value must be a valid unix timestamp in milliseconds, got '{firstDataParts[0]}'.");
    }

    private async Task WriteParquetAsync(string csvFilePath, string symbol)
    {
        var outputPath = Path.Combine(_parquetDirectory, "factors", $"{symbol}_factors.parquet");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);

        await using var connection = new DuckDBConnection(_connectionString);
        await connection.OpenAsync();

        // Read and transpose CSV data
        var transposedData = await TransposeCsvAsync(csvFilePath);

        // Create temporary table
        await CreateTemporaryFactorTable(connection, transposedData, symbol);

        // Copy to parquet
        await using var copyCmd = connection.CreateCommand();
        copyCmd.CommandText = $@"
            COPY (
                SELECT * FROM temp_factors
            )
            TO '{outputPath}'
            (FORMAT PARQUET);
        ";
        await copyCmd.ExecuteNonQueryAsync();

        // Drop temporary table
        await using var dropCmd = connection.CreateCommand();
        dropCmd.CommandText = "DROP TABLE IF EXISTS temp_factors;";
        await dropCmd.ExecuteNonQueryAsync();
    }

    private async Task<List<(long Timestamp, string FactorName, double Value)>> TransposeCsvAsync(string csvFilePath)
    {
        var result = new List<(long, string, double)>();

        using var reader = new StreamReader(csvFilePath);
        var headerLine = await reader.ReadLineAsync();
        if (headerLine == null)
            return result;

        var headers = CsvSplit(headerLine);
        var factorNames = headers.Skip(1).Select(h => h.Trim().Trim('"')).ToList();

        // Read each data row and transpose
        string? line;
        while ((line = await reader.ReadLineAsync()) != null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            var parts = CsvSplit(line);
            if (parts.Count < 2)
                continue;

            if (!long.TryParse(parts[0].Trim(), out var timestamp))
                continue;

            // For each factor column, create a row
            for (int i = 0; i < factorNames.Count; i++)
            {
                var factorName = factorNames[i];
                if (string.IsNullOrWhiteSpace(factorName))
                    continue;

                var valueStr = (i + 1 < parts.Count) ? parts[i + 1].Trim() : null;

                if (!string.IsNullOrWhiteSpace(valueStr) && 
                    double.TryParse(valueStr, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
                {
                    result.Add((timestamp, factorName, value));
                }
            }
        }

        return result;
    }

    private async Task CreateTemporaryFactorTable(DuckDBConnection connection, List<(long Timestamp, string FactorName, double Value)> data, string symbol)
    {
        // Create temporary table
        await using var createCmd = connection.CreateCommand();
        createCmd.CommandText = @"
            CREATE TEMPORARY TABLE temp_factors (
                timestamp BIGINT,
                symbol VARCHAR,
                name VARCHAR,
                value DOUBLE
            );
        ";
        await createCmd.ExecuteNonQueryAsync();

        // Insert data in batches
        const int batchSize = 5000;
        for (int i = 0; i < data.Count; i += batchSize)
        {
            var batch = data.Skip(i).Take(batchSize).ToList();
            var valuesList = string.Join(",", batch.Select(x => 
                $"({x.Timestamp}, '{EscapeSql(symbol)}', '{EscapeSql(x.FactorName)}', {x.Value})"));

            await using var insertCmd = connection.CreateCommand();
            insertCmd.CommandText = $"INSERT INTO temp_factors VALUES {valuesList};";
            await insertCmd.ExecuteNonQueryAsync();
        }
    }

    private static string EscapeSql(string input)
    {
        return input.Replace("'", "''");
    }

    private static List<string> CsvSplit(string line)
    {
        var result = new List<string>();
        var current = new System.Text.StringBuilder();
        var inQuotes = false;

        foreach (var c in line)
        {
            if (c == '"')
            {
                inQuotes = !inQuotes;
                current.Append(c);
            }
            else if (c == ',' && !inQuotes)
            {
                result.Add(current.ToString());
                current.Clear();
            }
            else
            {
                current.Append(c);
            }
        }

        result.Add(current.ToString());
        return result;
    }
}