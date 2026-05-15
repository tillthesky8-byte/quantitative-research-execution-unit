using Domain.Models;

namespace Core.Interfaces;
public interface ISlippageModel
{
    decimal ApplySlippage(decimal rawPrice, OrderRequest order, SymbolState symbolBar);
}