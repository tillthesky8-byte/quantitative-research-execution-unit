
using Core.Interfaces;
using Domain.Enums;
using Domain.Models;
using Core.States;
using Microsoft.Extensions.Logging;

namespace Core.Execution;

public class Broker : IBroker
{
    private readonly ILogger<Broker> _logger;

    private readonly ICommissionModel _commissionModel;
    private readonly ISlippageModel _slippageModel;

    private readonly List<Order>       _pendingOrders = [];
    private readonly List<TradeRecord> _tradeHistory = [];

    public IEnumerable<TradeRecord> TradeRecords => _tradeHistory.AsReadOnly();

    public Broker(ILogger<Broker> logger, ICommissionModel commissionModel, ISlippageModel slippageModel)
    {
        _logger          = logger;
        _commissionModel = commissionModel;
        _slippageModel   = slippageModel;
    }

    public void SubmitOrder(OrderRequest request, IPortfolio portfolio, long timestamp)
    {
        if (request.Quantity <= 0)
        {
            _logger.LogWarning("Attempted to submit order with non-positive quantity: {Quantity}", request.Quantity);
            return;
        }

        if (portfolio.Cash <= 0)
        {
            _logger.LogWarning("Attempted to submit order with negative cash balance: {Cash}", portfolio.Cash);
            return;
        }
        _logger.LogInformation("Submitting order: {Symbol} {Side} {Quantity} @ {Type} (Limit: {LimitPrice}, Stop: {StopPrice})",
            request.Symbol, request.Side, request.Quantity, request.Type, request.LimitPrice, request.StopPrice);
        var newOrder = new Order(request, timestamp);
        _pendingOrders.Add(newOrder);
    }

    public void ProcessOrders(MarketState marketState, IPortfolio portfolio)
    {
        var executedOrders = new List<Order>();

        foreach (var order in _pendingOrders)
        {
            var symbolBar = marketState.GetOhlcvForSymbol(order.Request.Symbol);

            bool triggered = order.Request.Type switch
            {
                OrderType.Market => true,
                OrderType.Limit  => order.Request.Side == OrderSide.Buy
                    ? symbolBar.Low  <= order.Request.LimitPrice
                    : symbolBar.High >= order.Request.LimitPrice,
                OrderType.Stop   => order.Request.Side == OrderSide.Buy
                    ? symbolBar.High >= order.Request.StopPrice
                    : symbolBar.Low  <= order.Request.StopPrice,
                
                _ => throw new InvalidOperationException($"Unsupported order type: {order.Request.Type}")
            };

            if (triggered)
            {
                ExecuteOrder(order, symbolBar, portfolio);
                executedOrders.Add(order);
            }
        }
        _pendingOrders.RemoveAll(executedOrders.Contains);

    }
    private void ExecuteOrder(Order order, SymbolState symbolBar, IPortfolio portfolio)
    {


        decimal rawPrice = order.Request.Type switch
        {
            OrderType.Market => (decimal)symbolBar.Close,
            OrderType.Limit  => (decimal)(order.Request.LimitPrice ?? throw new InvalidOperationException("Limit price must be set for limit orders.")),
            OrderType.Stop   => (decimal)symbolBar.Close, // For stop orders, we execute at market price once triggered

            _ => throw new InvalidOperationException($"Unsupported order type: {order.Request.Type}")
        };
        var executionPrice = _slippageModel.ApplySlippage(rawPrice, order.Request, symbolBar);
        var commission = _commissionModel.ComputeCommission(executionPrice, order.Request.Quantity);

        var tradeAction = portfolio.AdjustPosition
        (
            symbol       : order.Request.Symbol,
            quantityDelta: order.Request.Quantity * (order.Request.Side == OrderSide.Buy ? 1 : -1),
            price        : executionPrice,
            commission   : commission,
            timestamp    : order.Timestamp
        );

        
        var tradeRecord = new TradeRecord
        (
            Symbol    : order.Request.Symbol,
            Time      : order.Timestamp,
            Side      : (int)order.Request.Side,
            Quantity  : order.Request.Quantity,
            Price     : executionPrice,
            Commission: commission,
            Action    : tradeAction
        );
        _tradeHistory.Add(tradeRecord);

    }
        
}