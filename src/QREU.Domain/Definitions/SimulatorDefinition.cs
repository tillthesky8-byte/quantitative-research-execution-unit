using Domain.Enums;

namespace Domain.Definitions;
public sealed record SimulatorDefinition
(
    StrategyDefinition Strategy,
    SlippageType SlippageType,
    CommissionType CommissionType,
    decimal InitialCash
);