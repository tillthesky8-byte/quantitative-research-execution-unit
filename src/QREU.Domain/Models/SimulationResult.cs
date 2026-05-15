using System.Threading.Tasks.Dataflow;
using Domain.Other;
namespace Domain.Models;

public sealed record SimulationResult
(
    IReadOnlyList<EquityPoint>      EquityCurve,
    IReadOnlyList<TradeRecord>      TradeRecords,
    IReadOnlyList<RealizedPnlEvent> RealizedPnlEvents,

    MetricsSummary Metrics,

    DateTime StartTime,
    DateTime EndTime
)
{
    public decimal TotalCommission => TradeRecords.Sum(tr => tr.Commission);
    public override string ToString() => $"""
        {ConsoleColors.Cyan}Simulation Result: {StartTime:yyyy-MM-dd} -> {EndTime:yyyy-MM-dd}
        
        Performance:
        ────────────────────────────────────────────────────────────────
        Total Return:       {Metrics.TotalReturn:P2}
        Annualized Return:  {Metrics.AnnualizedReturn:P2}
        Max Drawdown:       {Metrics.MaxDrawdown:P2}


        Risk Metrics:
        ────────────────────────────────────────────────────────────────
        Sharpe Ratio:       {Metrics.SharpeRatio:F2}
        Sortino Ratio:      {Metrics.SortinoRatio:F2}


        Trade Statistics:
        ────────────────────────────────────────────────────────────────
        Win Rate:          {Metrics.WinRate:P2}
        Payoff Ratio:      {Metrics.PayoffRatio:F2}


        Activity/Scale:
        ────────────────────────────────────────────────────────────────
        Total Trades:      {TradeRecords.Count}
        Total Commission:   {TotalCommission:C2}
        """;
}

public sealed record MetricsSummary
(
    decimal TotalReturn,
    decimal AnnualizedReturn,
    decimal MaxDrawdown,
    decimal SharpeRatio,
    decimal SortinoRatio,
    decimal WinRate,
    decimal PayoffRatio
);