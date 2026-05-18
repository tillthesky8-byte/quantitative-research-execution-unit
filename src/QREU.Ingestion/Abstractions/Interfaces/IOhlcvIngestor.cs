namespace Ingestion.Interfaces;

public interface IOhlcvIngestor
{
    Task Ingest(string symbol);
}