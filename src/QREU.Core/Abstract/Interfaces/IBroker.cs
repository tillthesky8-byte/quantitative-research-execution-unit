using Domain.Models;
using Core.States;

namespace Core.Interfaces;

public interface IBroker
{
    IEnumerable<TradeRecord> TradeRecords { get; }
    void ProcessOrders(MarketState marketState, IPortfolio portfolio);
    void SubmitOrder(OrderRequest orderRequest, IPortfolio portfolio, long timestamp);
}