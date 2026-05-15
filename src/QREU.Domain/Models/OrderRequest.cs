using Domain.Enums;

namespace Domain.Models;

public sealed class OrderRequest
{
    public required string Symbol { get; init; }
    public required decimal Quantity { get; set;}
    public required OrderSide Side { get; set; }
    public required OrderType Type { get; init; }
    public decimal? LimitPrice { get; init; }
    public decimal? StopPrice { get; init; }
}