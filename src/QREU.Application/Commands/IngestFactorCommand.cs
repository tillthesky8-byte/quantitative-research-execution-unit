
using System.CommandLine;
using Application.Models;
using Application.Options;
using Application.Runners;
using Domain.Models;
using Microsoft.Extensions.Logging;
namespace Application.Commands;
public sealed class IngestFactorCommand : Command
{
    public IngestFactorCommand(AppSettings appSettings, ILogger<IngestFactorCommand> logger, ILoggerFactory loggerFactory) : base("ingest-factor", "Ingest factor data from CSV files into DuckDB")
    {
        var symbolsOption = OptionFactory.instrumentsOption;
        Add(symbolsOption);

        SetAction(async (context) =>
        {
            var symbols         = context.GetValue(symbolsOption) ?? throw new InvalidOperationException("At least one factor must be specified for ingestion.");
            var connectionString = appSettings.ConnectionString;    
            var csvDirectory     = appSettings.CsvRoot             ?? throw new InvalidOperationException("CSV root directory is not configured.");
            var parquetDirectory = appSettings.ParquetRoot         ?? throw new InvalidOperationException("Parquet root directory is not configured.");

            var ingestConfiguration = new IngestConfiguration(
                connectionString: connectionString,
                csvDirectory: csvDirectory,
                parquetDirectory: parquetDirectory,
                symbols: [.. symbols.Select(s => s.Symbol.ToUpper())]
            );


            var runner = new IngestFactorRunner(ingestConfiguration, loggerFactory.CreateLogger<IngestFactorRunner>(), loggerFactory);

            await runner.RunAsync();
        });
    }

        
}    