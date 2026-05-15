using System.Data.Common;
using Domain.Interfaces;
using Domain.Models;
using Domain.Other;
using Microsoft.Extensions.Logging;

namespace Core.Stream;

public sealed class Streamer : IStreamer
{
    private readonly ILogger<Streamer>? _logger;
    private readonly DbDataReader _ohlcvReader;
    private readonly DbDataReader _factorReader;
    private readonly DbCommand _ohlcvCommand;
    private readonly DbCommand _factorCommand;
    private readonly DbConnection _connection;
    private bool _disposed;

    public Streamer(
        DbDataReader ohlcvReader,
        DbDataReader factorReader,
        DbCommand ohlcvCommand,
        DbCommand factorCommand,
        DbConnection connection,
        ILogger<Streamer>? logger)
    {
        _ohlcvReader   = ohlcvReader;
        _factorReader  = factorReader;
        _ohlcvCommand  = ohlcvCommand;
        _factorCommand = factorCommand;
        _connection    = connection;
        _logger        = logger;
    }
    public async IAsyncEnumerable<IMarketEvent> StreamAsync()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(Streamer));

        try
        {
            var ohlcvEvent = await ReadNextOhlcvAsync();
            var factorEvent = await ReadNextFactorAsync();

            while (ohlcvEvent is not null || factorEvent is not null)
            {
                if (factorEvent is null || (ohlcvEvent is not null && ohlcvEvent.Timestamp <= factorEvent.Timestamp))
                {
                    yield return ohlcvEvent!;
                    ohlcvEvent = await ReadNextOhlcvAsync();
                }
                else
                {
                    yield return factorEvent!;
                    factorEvent = await ReadNextFactorAsync();
                }
            }
        }
        finally
        {
            await DisposeAsync();
        }
    }

    private async ValueTask DisposeInternalAsync()
    {
        if (_disposed)
            return;

        _disposed = true;

        await _ohlcvReader.DisposeAsync();
        await _factorReader.DisposeAsync();
        _ohlcvCommand.Dispose();
        _factorCommand.Dispose();
        await _connection.DisposeAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await DisposeInternalAsync();
    }

    private async Task<OhlcvEvent?> ReadNextOhlcvAsync()
    {
        if (await _ohlcvReader.ReadAsync())
        {
            return new OhlcvEvent
            {
                Timestamp = _ohlcvReader.GetInt64(0),
                Symbol = _ohlcvReader.GetString(1),
                Open = Convert.ToDecimal(_ohlcvReader.GetValue(2)),
                High = Convert.ToDecimal(_ohlcvReader.GetValue(3)),
                Low = Convert.ToDecimal(_ohlcvReader.GetValue(4)),
                Close = Convert.ToDecimal(_ohlcvReader.GetValue(5)),
                Volume = Convert.ToDecimal(_ohlcvReader.GetValue(6))
            };
        }
        return null;
    }

    private async Task<FactorEvent?> ReadNextFactorAsync()
    {
        if (await _factorReader.ReadAsync())
        {
            return new FactorEvent
            {
                Timestamp = _factorReader.GetInt64(0),
                Symbol = _factorReader.GetString(1),
                Factor = _factorReader.GetString(2),
                Value = Convert.ToDecimal(_factorReader.GetValue(3))
            };
        }
        return null;
    }

}