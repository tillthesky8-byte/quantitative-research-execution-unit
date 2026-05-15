using Domain.Enums;
namespace Domain.Models;

public sealed record RealizedPnlEvent
(
    string Symbol,
    long Timestamp,
    decimal Quantity,
    decimal RealizedPnl,
    decimal Commission,
    decimal EntryPrice,
    decimal ExitPrice
);
    