using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface ITradeRepository
{
    Task<List<Trade>> GetTradesAsync(Guid runId, DateTime from, DateTime to);
}