using Core.Execution;
using Core.Interfaces;
using Domain.Enums;
using Microsoft.Extensions.Logging;

namespace Core.Factories;
public static class BrokerFactory
{
    public static IBroker CreateBroker(SlippageType slippageType, CommissionType commissionType, ILoggerFactory loggerFactory)
    {
        var slippageModel = slippageType switch
        {
            SlippageType.Default => new DefaultSlippageModel(),
            _ => throw new ArgumentException($"Unsupported slippage type: {slippageType}")
        };

        var CommissionModel = commissionType switch
        {
            CommissionType.Default => new DefaultCommissionModel(),
            _ => throw new ArgumentException($"Unsupported commission type: {commissionType}")
        };

        return new Broker(loggerFactory.CreateLogger<Broker>(), CommissionModel, slippageModel);
    }
}