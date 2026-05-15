using System.Security.Cryptography.X509Certificates;
using Domain.Enums;

namespace Domain.Models;

public record TradeRecord
(
    string Symbol,
    long Time,
    int Side,
    decimal Quantity,
    decimal Price,
    decimal Commission,
    TradeAction Action
);