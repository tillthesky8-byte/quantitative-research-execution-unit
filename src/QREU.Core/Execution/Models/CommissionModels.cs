using Core.Interfaces;

namespace Core.Execution;

public class DefaultCommissionModel : ICommissionModel
{
    public decimal ComputeCommission(decimal fillPrice, decimal quantity) => 0m;
}