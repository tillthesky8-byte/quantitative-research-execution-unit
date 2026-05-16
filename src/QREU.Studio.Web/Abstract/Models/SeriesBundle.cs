namespace Studio.Web.Models;

public record SeriesBundle
(
    IEnumerable<Ohlc> Ohlc,
    IEnumerable<EquityPoint> EquityCurve
);