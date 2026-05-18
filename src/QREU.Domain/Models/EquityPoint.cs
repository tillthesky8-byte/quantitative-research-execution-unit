namespace Domain.Models;

public record EquityPoint
(
    long    Time,
    decimal Equity,
    decimal Cash
);