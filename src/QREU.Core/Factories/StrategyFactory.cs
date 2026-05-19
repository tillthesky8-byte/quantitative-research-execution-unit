using Core.Interfaces;
using Modules.Strategies;
namespace Core.Factories;

public static class StrategyFactory
{
    public static IStrategy CreateStrategy(StrategyType strategyType, IReadOnlyDictionary<string, string> parameters)
    {
        return strategyType switch
        {
            StrategyType.BBB => new CustomBBB(parameters),
            StrategyType.BollingerBondsBreakout => new BollingerBondsBreakout(parameters),
            StrategyType.BollingerBondsReversion => new BollingerBondsReversion(parameters),
            _ => throw new ArgumentException($"Unsupported strategy type: {strategyType}")
        };
    }
}