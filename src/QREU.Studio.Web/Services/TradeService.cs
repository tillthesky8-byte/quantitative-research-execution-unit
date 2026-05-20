using Studio.Web.Interfaces;
using Studio.Web.Models;

namespace Studio.Web.Services;

public class TradeService : ITradeService
{
    private readonly ITradeRepository _tradeRepository;

    public TradeService(ITradeRepository tradeRepository)
    {
        _tradeRepository = tradeRepository;
    }

    public async Task<List<Trade>> GetTradesAsync(Guid runId, int page = 1, int pageSize = 100)
    {
        return await _tradeRepository.GetTradesAsync(runId, page, pageSize);
    }

    public async Task<List<Mark>> GetMarksAsync(Guid runId, int page = 1, int pageSize = 100)
    {
        var trades = await _tradeRepository.GetTradesAsync(runId, page, pageSize);

        var marks = trades.Select(trade => new Mark(
            Time: trade.Time / 1000, // Convert milliseconds to seconds for the frontend
            Position: trade.Side == "BUY" ? "belowBar" : "aboveBar",
            Color: trade.Side == "BUY" ? "Green" : "Red",
            Shape: "Circle",
            Text: $"{trade.Action} {trade.Quantity} @ {trade.Price}"
        )).ToList();

        return marks;
    }
}