using Domain.Models;
using Core.States;
namespace Core.Interfaces;
public interface IStrategy
{
    IEnumerable<OrderRequest> OnTick(IPortfolio portfolio, MarketState marketState);
}