
using Domain.Enums;
using Domain.Interfaces;
using Domain.Models;
using Core.States;
using Core.Interfaces;

namespace Modules.Families;

public abstract class ExitEntryStrategy : IStrategy
{
    protected abstract bool OpenLongCondition(SymbolState state);
    protected abstract bool OpenShortCondition(SymbolState state);
    protected abstract bool CloseLongCondition(SymbolState state);
    protected abstract bool CloseShortCondition(SymbolState state);
    protected abstract IIndicator[] Indicators { get; }
    private decimal ComputeQuantity(decimal price, IPortfolio portfolio)
    {
        decimal allocation = 0.5m * portfolio.Cash; // Risk 50% of available cash per trade
        decimal quantity = Math.Floor(allocation / price);
        return quantity > 0 ? quantity : throw new InvalidOperationException("Not enough cash to open a position");
    }
    public IEnumerable<OrderRequest> OnTick(IPortfolio portfolio, MarketState marketState)
    {
        foreach(var (symbol, bar) in marketState.Symbols)
        {
            foreach (var indicator in Indicators)
                indicator.Update(marketState.Symbols);
            
            var hasPosition = portfolio.GetPosition(symbol, out var position);

            if (!hasPosition || position is null || position.Quantity == 0)
            {
                if (OpenLongCondition(bar))
                    yield return new OrderRequest() { Symbol = symbol, Side = OrderSide.Buy, Type = OrderType.Market, Quantity = ComputeQuantity(bar.Close, portfolio) };

                else if (OpenShortCondition(bar))
                    yield return new OrderRequest() { Symbol = symbol, Side = OrderSide.Sell, Type = OrderType.Market, Quantity = ComputeQuantity(bar.Close, portfolio) };
            }
            else
            {
                if (position.Quantity > 0)
                {
                    if (CloseLongCondition(bar))
                        yield return new OrderRequest() { Symbol = symbol, Side = OrderSide.Sell, Type = OrderType.Market, Quantity = position.Quantity };
                }
                else if (position.Quantity < 0)
                {
                    if (CloseShortCondition(bar))
                        yield return new OrderRequest() { Symbol = symbol, Side = OrderSide.Buy, Type = OrderType.Market, Quantity = Math.Abs(position.Quantity) };
                }
            }
        }
    }
}