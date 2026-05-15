using Domain.Enums;
using Domain.Models;
using Core.States;

namespace Core.Interfaces;
public interface IPortfolio
{
    decimal Cash { get; }
    Dictionary<string, Position> Positions { get; }
    IEnumerable<RealizedPnlEvent> RealizedPnlEvents { get; }
    decimal GetEquity(MarketState marketState);
    bool GetPosition(string symbol, out Position? position);
    TradeAction AdjustPosition(string symbol, decimal quantityDelta, decimal price, decimal commission, long timestamp);
}
