using Domain.Enums;
using Core.Interfaces;
using Domain.Models;
using Microsoft.Extensions.Logging;
using Domain.Other;

namespace Core.States;

public sealed class Portfolio(ILogger<Portfolio>? logger, decimal initialCash = 100000) : IPortfolio
{
    private readonly ILogger<Portfolio>? _logger = logger;

    private decimal                      _cash              = initialCash;
    private Dictionary<string, Position> _positions         = new(StringComparer.OrdinalIgnoreCase);
    private List<RealizedPnlEvent>       _realizedPnlEvents = [];
    
    public decimal                       Cash => _cash;
    public Dictionary<string, Position>  Positions => _positions;
    public IEnumerable<RealizedPnlEvent> RealizedPnlEvents => _realizedPnlEvents;


    public TradeAction AdjustPosition(string symbol, decimal quantityDelta, decimal price, decimal commission, long timestamp)
    {
        if (quantityDelta == 0) throw new ArgumentException("Quantity delta cannot be zero. Review the strategy logic.", nameof(quantityDelta));
       
        GetPosition(symbol, out Position? existingPosition);

        if (existingPosition is null) return OpenPosition(symbol, quantityDelta, price, commission);

        if      (IsFullClose(existingPosition.Quantity, quantityDelta)) return ClosePosition   (existingPosition, quantityDelta, price, commission, timestamp);
        else if (IsReversal (existingPosition.Quantity, quantityDelta)) return ReversePosition (existingPosition, quantityDelta, price, commission, timestamp);
        else if (IsScaleIn  (existingPosition.Quantity, quantityDelta)) return ScaleInPosition (existingPosition, quantityDelta, price, commission);
        else if (IsScaleOut (existingPosition.Quantity, quantityDelta)) return ScaleOutPosition(existingPosition, quantityDelta, price, commission, timestamp);
        
        else throw new InvalidOperationException($"Invalid position adjustment scenario for symbol {symbol} with existing quantity {existingPosition.Quantity} and quantity delta {quantityDelta}. Review the strategy logic to ensure correct position adjustments.");
    }

    public decimal GetEquity(MarketState marketState) 
        => _positions.Values.Sum(p => p.Quantity * marketState.GetPriceForSymbol(p.Symbol)) + _cash;

    public bool GetPosition(string symbol, out Position? position) => _positions.TryGetValue(symbol, out position);
    private TradeAction OpenPosition(string symbol, decimal quantityDelta, decimal price, decimal commission)
    {
        _cash += CalculateCashFlow(quantityDelta, price, commission);
        _positions[symbol] = new Position
        {
            Symbol            = symbol,
            Quantity          = quantityDelta,
            AverageEntryPrice = price,
        };
        _logger?.LogDebug(LogMessages.PositionOpened, symbol, quantityDelta, price);
        return TradeAction.Open;
    }

    private TradeAction ClosePosition(Position existingPosition, decimal quantityDelta, decimal price, decimal commission, long timestamp)
    {
        if (-quantityDelta != existingPosition.Quantity)
            throw new InvalidOperationException($"Attempting to close position with quantity delta {quantityDelta} that does not match existing position quantity {existingPosition.Quantity}. This should be a full close scenario.");
       
        _cash += CalculateCashFlow(quantityDelta, price, commission);
        _positions.Remove(existingPosition.Symbol);

        var pnl = existingPosition.GetUnrealizedPnL(price);
        _realizedPnlEvents.Add(new RealizedPnlEvent
        (
            Symbol      : existingPosition.Symbol,
            Timestamp   : timestamp,
            RealizedPnl : pnl,
            Quantity    : existingPosition.Quantity,
            Commission  : commission,
            EntryPrice  : existingPosition.AverageEntryPrice,
            ExitPrice   : price
        ));

        _logger?.LogDebug(LogMessages.PositionClosed, existingPosition.Symbol, existingPosition.Quantity, existingPosition.AverageEntryPrice, pnl, commission, price);
        return TradeAction.Close;
    }

    private TradeAction ScaleInPosition(Position existingPosition, decimal quantityDelta, decimal price, decimal commission)
    {
        if (Math.Sign(existingPosition.Quantity) != Math.Sign(quantityDelta))
            throw new InvalidOperationException($"Attempting to scale in position with quantity delta {quantityDelta} that has opposite sign of existing position quantity {existingPosition.Quantity}. This should be a reversal scenario.");
        
        _cash += CalculateCashFlow(quantityDelta, price, commission);

        var newQuantity = existingPosition.Quantity + quantityDelta;
        existingPosition.AverageEntryPrice = (existingPosition.AverageEntryPrice * existingPosition.Quantity + price * quantityDelta) / newQuantity;
        existingPosition.Quantity = newQuantity;

        return TradeAction.ScaleIn;
    }

    private TradeAction ScaleOutPosition(Position existingPosition, decimal quantityDelta, decimal price, decimal commission, long timestamp)
    {
        if (Math.Sign(existingPosition.Quantity) == Math.Sign(quantityDelta))
            throw new InvalidOperationException($"Attempting to scale out position with quantity delta {quantityDelta} that has same sign as existing position quantity {existingPosition.Quantity}. This should be a scale in scenario.");

        if (Math.Abs(quantityDelta) >= Math.Abs(existingPosition.Quantity))
            throw new InvalidOperationException($"Attempting to scale out position with quantity delta {quantityDelta} that is greater than or equal to existing position quantity {existingPosition.Quantity}. This should be a close or reversal scenario.");
        
        _cash += CalculateCashFlow(quantityDelta, price, commission);

        var newQuantity = existingPosition.Quantity + quantityDelta;
        var pnl = -quantityDelta * (price - existingPosition.AverageEntryPrice);

        _realizedPnlEvents.Add(new RealizedPnlEvent
        (
            Symbol      : existingPosition.Symbol,
            Timestamp   : timestamp,
            RealizedPnl : pnl,
            Quantity    : -quantityDelta,
            Commission  : commission,
            EntryPrice  : existingPosition.AverageEntryPrice,
            ExitPrice   : price
        ));

        existingPosition.Quantity = newQuantity;
        return TradeAction.ScaleOut;
    }

    private TradeAction ReversePosition(Position existingPosition, decimal quantityDelta, decimal price, decimal commission, long timestamp)
    {
        if (Math.Sign(existingPosition.Quantity) == Math.Sign(quantityDelta))
            throw new InvalidOperationException($"Attempting to reverse position with quantity delta {quantityDelta} that has same sign as existing position quantity {existingPosition.Quantity}. This should be a scale in scenario.");

        if (Math.Abs(quantityDelta) <= Math.Abs(existingPosition.Quantity))
            throw new InvalidOperationException($"Attempting to reverse position with quantity delta {quantityDelta} that is less than or equal to existing position quantity {existingPosition.Quantity}. This should be a close or scale out scenario.");
        
        ClosePosition(existingPosition, -existingPosition.Quantity, price, commission, timestamp);
        OpenPosition(existingPosition.Symbol, quantityDelta + existingPosition.Quantity, price, commission);
        return TradeAction.Reverse;
    }
    private decimal CalculateCashFlow(decimal quantityDelta, decimal price, decimal commission)
    {
        decimal grossCashFlow = -quantityDelta * price;
        return grossCashFlow - commission;
    }

    private bool IsFullClose(decimal existingQuantity, decimal quantityDelta)
        => Math.Abs(existingQuantity + quantityDelta) < 0.0000001m;

    private bool IsReversal(decimal existingQuantity, decimal quantityDelta)
        => Math.Sign(existingQuantity) != Math.Sign(quantityDelta)
           && Math.Abs(quantityDelta) > Math.Abs(existingQuantity);

    private bool IsScaleIn(decimal existingQuantity, decimal quantityDelta)
        => Math.Sign(existingQuantity) == Math.Sign(quantityDelta);

    private bool IsScaleOut(decimal existingQuantity, decimal quantityDelta)
        => Math.Sign(existingQuantity) != Math.Sign(quantityDelta)
           && Math.Abs(quantityDelta) < Math.Abs(existingQuantity);
}