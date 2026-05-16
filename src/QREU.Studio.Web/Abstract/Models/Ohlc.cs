namespace Studio.Web.Models;

public record Ohlc
(
    long   Time,
    double Open,
    double High,
    double Low,
    double Close
);