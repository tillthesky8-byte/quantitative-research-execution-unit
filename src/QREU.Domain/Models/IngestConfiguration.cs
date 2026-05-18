using Domain.Definitions;

namespace Domain.Models;

public sealed class IngestConfiguration
{
    public string ConnectionString { get; init; }
    public string CsvDirectory { get; init; }
    public string ParquetDirectory { get; init; }
    public string[] Symbols { get; init; }
    public IngestConfiguration(string connectionString, string csvDirectory, string parquetDirectory, string[] symbols)
    {
        ConnectionString = connectionString;
        CsvDirectory     = csvDirectory;
        ParquetDirectory = parquetDirectory;
        Symbols          = symbols;
    }
}