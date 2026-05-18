using Core.Interfaces;

using Domain.Models;
using Domain.Other;

namespace Core.Recording;

public class Recorder : IRecorder
{
    private readonly List<EquityPoint>      _equityCurve       = [];
    private readonly List<TradeRecord>      _tradeRecords      = [];
    private readonly List<RealizedPnlEvent> _realizedPnlEvents = [];
    public void Record(long timestamp, decimal equity, decimal cash) {
        _equityCurve.Add(new EquityPoint(timestamp, equity, cash));
    }

    public void AppendTrades(IEnumerable<TradeRecord> trades) =>
        _tradeRecords.AddRange(trades);

    public void AppendRealizedPnlEvents(IEnumerable<RealizedPnlEvent> events) =>
        _realizedPnlEvents.AddRange(events);

    
    public SimulationResult BuildResult()
    {
        if (_equityCurve.Count < 2) throw new InvalidOperationException("At least two equity points are required to build a simulation result.");

        var _equityCurveDaily = _equityCurve
            .GroupBy(ep => new DateTimeOffset(DateTimeOffset.FromUnixTimeMilliseconds(ep.Time).Date, TimeSpan.Zero))
            .Select(g => new EquityPoint(g.Key.ToUnixTimeMilliseconds(), g.Last().Equity, g.Last().Cash))
            .ToList();

        var initialEquity = _equityCurveDaily.First().Equity;
        var finalEquity   = _equityCurveDaily.Last().Equity;
        var startTime     = DateTimeOffset.FromUnixTimeMilliseconds(_equityCurveDaily.First().Time);
        var endTime       = DateTimeOffset.FromUnixTimeMilliseconds(_equityCurveDaily.Last().Time);
        var years         = (decimal)(endTime - startTime).TotalDays / 365.25m;  

        var totalReturn      = (finalEquity - initialEquity) / initialEquity;
        var annualizedReturn = DecimalMath.Pow(1m + totalReturn, 1m / years) - 1m;
        var maxDrawdown      = ComputeMaxDrawdown(_equityCurveDaily);

        var sharpeRatio      = ComputeSharpeRatio(_equityCurveDaily);
        var sortinoRatio     = ComputeSortinoRatio(_equityCurveDaily);

        var winRate          = ComputeWinRate(_realizedPnlEvents);
        var payoffRatio      = ComputePayoffRatio(_realizedPnlEvents);

        var metrics = new MetricsSummary
        (
            TotalReturn      : totalReturn,
            AnnualizedReturn : annualizedReturn,
            MaxDrawdown      : maxDrawdown,
            SharpeRatio      : sharpeRatio,
            SortinoRatio     : sortinoRatio,
            WinRate          : winRate,
            PayoffRatio      : payoffRatio
        );


        return new SimulationResult
        (
            Metrics            : metrics,
            EquityCurve        : _equityCurve,
            TradeRecords       : _tradeRecords,
            RealizedPnlEvents  : _realizedPnlEvents,
            StartTime          : startTime.DateTime,
            EndTime            : endTime.DateTime
        );


    }

    private decimal ComputeMaxDrawdown(List<EquityPoint> equityCurve)
    {
        decimal maxDrawdown = 0m;
        decimal peak = equityCurve[0].Equity;

        foreach (var point in equityCurve)
        {
            if (point.Equity > peak)
            {
                peak = point.Equity;
            }
            else
            {
                var drawdown = (peak - point.Equity) / peak;
                if (drawdown > maxDrawdown)
                {
                    maxDrawdown = drawdown;
                }
            }
        }

        return maxDrawdown;
    }
    private decimal ComputeSharpeRatio(List<EquityPoint> equityCurveDaily)
    {
        var returns = SimpleReturns(equityCurveDaily.Select(ep => ep.Equity));

        var averageReturn = returns.Average();
        var returnStdDev = StandardDeviation(returns);

        return returnStdDev == 0 ? 0m : averageReturn / returnStdDev * DecimalMath.Sqrt(252m); 
    }
    private decimal ComputeSortinoRatio(List<EquityPoint> equityCurveDaily)
    {
        var returns = SimpleReturns(equityCurveDaily.Select(ep => ep.Equity));


        var averageReturn = returns.Average();
        var downsideReturns = returns.Where(r => r < 0).ToList();
        var downsideStdDev = StandardDeviation(downsideReturns);

        return downsideStdDev == 0 ? 0m : averageReturn / downsideStdDev * DecimalMath.Sqrt(252m); 
    }
    private decimal StandardDeviation(List<decimal> values)
    {
        var mean = values.Average();
        var variance = values.Average(v => (v - mean) * (v - mean));
        return DecimalMath.Sqrt(variance);
    }
    private decimal ComputeWinRate(List<RealizedPnlEvent> pnlEvents)
    {
        if (pnlEvents.Count == 0) return 0m;
        var wins = pnlEvents.Count(e => e.RealizedPnl > 0);
        return (decimal)wins / pnlEvents.Count;
    }
    private decimal ComputePayoffRatio(List<RealizedPnlEvent> pnlEvents)
    {
        var winningTrades = pnlEvents.Where(e => e.RealizedPnl > 0).ToList();
        var losingTrades  = pnlEvents.Where(e => e.RealizedPnl < 0).ToList();
        if (winningTrades.Count == 0 || losingTrades.Count == 0) return 0m;
        var averageWin = winningTrades.Average(e => e.RealizedPnl);
        var averageLoss = losingTrades.Average(e => e.RealizedPnl);
        return averageLoss == 0 ? 0m : averageWin / Math.Abs(averageLoss);
    }
    private List<decimal> SimpleReturns(IEnumerable<decimal> equityValues)
    {
        var returns = new List<decimal>();
        decimal? previous = null;
        foreach (var equity in equityValues)
        {
            if (previous.HasValue)
            {
                returns.Add((equity - previous.Value) / previous.Value);
            }
            previous = equity;
        }
        return returns;
    }
}