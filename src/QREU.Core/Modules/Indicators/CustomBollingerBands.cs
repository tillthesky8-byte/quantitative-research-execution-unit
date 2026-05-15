using Domain.Interfaces;
using Domain.Models;
using Domain.Other;

namespace Modules.Indicators;

public sealed class CustomBollingerBands : IIndicator
{
    private class SymbolBands
    {
        public Queue<decimal> MiddleBandWindow { get; } = [];
        public decimal MiddleBandSum { get; set; } = 0m;
        public decimal MiddleBandSquaredSum { get; set; } = 0m;
        public Queue<decimal> SideBandWindow { get; } = [];
        public decimal SideBandSum { get; set; } = 0m;
        public decimal SideBandSquaredSum { get; set; } = 0m;

        public decimal UpperBand { get; set; } = 0m;
        public decimal LowerBand { get; set; } = 0m;
        public decimal MiddleBand { get; set; } = 0m;
    }

    private readonly int _middlePeriod;
    private readonly int _sidePeriod;
    private readonly decimal _stdDevMultiplier;
    private readonly string _source;
    private bool _isReady = false;

    private readonly Dictionary<string, SymbolBands> _bands = [];

    public decimal UpperBand(string symbol) => _bands.TryGetValue(symbol, out var b) ? b.UpperBand : 0m;
    public decimal MiddleBand(string symbol) => _bands.TryGetValue(symbol, out var b) ? b.MiddleBand : 0m;
    public decimal LowerBand(string symbol) => _bands.TryGetValue(symbol, out var b) ? b.LowerBand : 0m;

    public bool IsReady => _isReady;

    public CustomBollingerBands(int middlePeriod, int sidePeriod, decimal stdDevMultiplier, string source)
    {
        _middlePeriod = middlePeriod;
        _sidePeriod = sidePeriod;
        _stdDevMultiplier = stdDevMultiplier;
        _source = source;
    }
    
    public void Update(IReadOnlyDictionary<string, SymbolState> symbolStates)
    {
        foreach (var (symbol, bar) in symbolStates)
        {
            if (!_bands.TryGetValue(symbol, out var b))
            {
                b = new SymbolBands();
                _bands[symbol] = b;
            }

            decimal value = bar[_source];

            b.MiddleBandWindow.Enqueue(value);
            b.MiddleBandSum += value;
            b.MiddleBandSquaredSum += value * value;

            if (b.MiddleBandWindow.Count > _middlePeriod)
            {
                decimal old = b.MiddleBandWindow.Dequeue();
                b.MiddleBandSum -= old;
                b.MiddleBandSquaredSum -= old * old;
            }

            b.SideBandWindow.Enqueue(value);
            b.SideBandSum += value;
            b.SideBandSquaredSum += value * value;

            if (b.SideBandWindow.Count > _sidePeriod)
            {
                decimal old = b.SideBandWindow.Dequeue();
                b.SideBandSum -= old;
                b.SideBandSquaredSum -= old * old;
            }

            if (b.MiddleBandWindow.Count == _middlePeriod && b.SideBandWindow.Count == _sidePeriod)
            {
                b.MiddleBand = b.MiddleBandSum / _middlePeriod;

                decimal sideVariance = (b.SideBandSquaredSum - (b.SideBandSum * b.SideBandSum / _sidePeriod)) / _sidePeriod;
                decimal sideStdDev = DecimalMath.Sqrt(sideVariance);

                b.UpperBand = b.MiddleBand + _stdDevMultiplier * sideStdDev;
                b.LowerBand = b.MiddleBand - _stdDevMultiplier * sideStdDev;
                _isReady = true;
            }
        }
    }
}