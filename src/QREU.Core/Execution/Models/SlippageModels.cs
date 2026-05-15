using Core.Interfaces;
using Domain.Models;

namespace Core.Execution;

public class DefaultSlippageModel : ISlippageModel
{
    public decimal ApplySlippage(decimal rawPrice, OrderRequest order, SymbolState symbolBar) => rawPrice;
}