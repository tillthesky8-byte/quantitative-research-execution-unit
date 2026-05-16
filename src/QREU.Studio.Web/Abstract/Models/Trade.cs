namespace Studio.Web.Models;

public record Trade
(
    long Time,
    string Symbol,
    string Side,
    string Action,
    double Price,
    double Quantity,
    double Commission
);