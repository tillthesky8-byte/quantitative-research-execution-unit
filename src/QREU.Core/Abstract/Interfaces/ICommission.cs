namespace Core.Interfaces;

public interface ICommissionModel
{
    decimal ComputeCommission(decimal fillPrice, decimal quantity);
}