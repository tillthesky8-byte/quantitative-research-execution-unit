using Domain.Interfaces;
using Domain.Models;
using Domain.Other;

namespace Modules.Indicators;

public sealed class SMA : IIndicator
{
    private class SymbolSMA
    {
        public Queue<decimal> Window { get; } = [];
        public decimal Sum { get; set; } = 0m;
        public decimal Value { get; set; } = 0m;
    }

    private readonly int _period;
    private readonly string _source;
    private bool _isReady = false;

    private readonly Dictionary<string, SymbolSMA> _smas = [];

    public decimal GetValue(string symbol) => _smas.TryGetValue(symbol, out var s) ? s.Value : 0m;

    public bool IsReady => _isReady;

    public SMA(int period, string source)
    {
        _period = period;
        _source = source;
    }

    public void Update(IReadOnlyDictionary<string, SymbolState> symbolStates)
    {
        foreach (var (symbol, bar) in symbolStates)
        {
            if (!_smas.TryGetValue(symbol, out var s))
            {
                s = new SymbolSMA();
                _smas[symbol] = s;
            }

            decimal value = bar[_source];

            s.Window.Enqueue(value);
            s.Sum += value;

            if (s.Window.Count > _period)
            {
                decimal removed = s.Window.Dequeue();
                s.Sum -= removed;
            }

            if (s.Window.Count == _period)
            {
                s.Value = s.Sum / _period;
                _isReady = true;
            }
        }
    }
}
    