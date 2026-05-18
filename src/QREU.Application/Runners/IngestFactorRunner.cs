using Application.Models;
using Domain.Models;
using Ingestion.Ingestors;
using Microsoft.Extensions.Logging;
namespace Application.Runners;
public sealed class IngestFactorRunner
{
    private readonly IngestConfiguration _configuration;
    private readonly ILogger<IngestFactorRunner>? _logger;
    private readonly ILoggerFactory _loggerFactory;

    public IngestFactorRunner(IngestConfiguration configuration, ILogger<IngestFactorRunner>? logger = null, ILoggerFactory? loggerFactory = null)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger        = logger;
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }

    public async Task RunAsync()
    {
        if (_configuration.Symbols == null || !_configuration.Symbols.Any())
        {
            _logger?.LogWarning("No symbols provided for ingestion. Skipping factor ingestion.");
            return;
        }
        var ingestor = new FactorIngestor(_configuration.ConnectionString, _configuration.CsvDirectory, _configuration.ParquetDirectory);

        foreach (var symbol in _configuration.Symbols)
        {
            _logger?.LogInformation("Starting ingestion for symbol: {Symbol}", symbol);
            try
            {
                await ingestor.Ingest(symbol.ToUpper());
                _logger?.LogInformation("Completed ingestion for symbol: {Symbol}", symbol);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Error ingesting data for symbol: {Symbol}", symbol);
            }
        }
    }
}