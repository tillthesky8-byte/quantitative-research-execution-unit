using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface ITradeService
{
    Task<List<Trade>> GetTradesAsync(Guid runId, int page = 1, int pageSize = 100);
    Task<List<Mark>> GetMarksAsync(Guid runId, int page = 1, int pageSize = 100);
}