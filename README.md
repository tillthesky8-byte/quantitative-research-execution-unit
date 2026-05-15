# QREU (Quantitative Research Execution Unit)

QREU is a high-performance, event-driven trading simulation and backtesting framework built in modern .NET (C#). It evaluates trading strategies against historical price behaviors and custom external factors using an ultra-low-memory data streaming approach.

## 🏗 Architecture

The framework handles complexities such as commission, slippage, and portfolio tracking while exposing decoupled interfaces to focus strictly on quantitative strategy development. 

The project structure is broken down into modular domain concerns:
*   **`QREU.Application`**: CLI engine, configuration models (`RunConfiguration`, YAML bindings), logging (`CustomConsoleFormatter`), and Dependency Injection orchestration via `RootRunner`.
*   **`QREU.Core`**: The simulation backbone, defining interfaces (`IBroker`, `IStrategy`, `IPortfolio`, `IIndicator`) and hosting the `Simulator` event-loop engine.
*   **`QREU.Domain`**: High-availability business boundaries, abstractions (`IMarketEvent`, `RunConfiguration`, `OhlcvEvent`), and Enums (Order Types, Sides, Action Definitions).
*   **`QREU.Writer`**: Pipeline offloading allowing seamless data persistence for executed backtesting results, historical trade logs, and equity curves.

## ✨ Core Features

*   **Streaming Execution Engine**: Leverages `IAsyncEnumerable` over underlying `DbDataReader` cursors to lazily load and merge OHLCV candles and custom mathematical factors from standard datasets. No full-dataset memory overhead.
*   **Event-Driven Pipeline**: Resolves portfolios, broker actions, and strategy on tick-time progressions `marketEvent.Timestamp`.
*   **Pluggable Broker Extensions**: Abstracts trading environment constraints, seamlessly injecting dynamically calculated Commission and Slippage models based on configurations.
*   **Extensible Indicators Factory**: Allows wrapping logic for simple and complex numeric indicators (like Custom Bollinger Bands).
*   **CLI-Native**: Ships out-of-the-box leveraging `System.CommandLine`, allowing execution tuning via CLI arguments or configuration matrices (e.g., overriding settings in `sample.yaml` or `appsettings.json`).

## 🚀 Engine Cycle Strategy

When simulating equity histories, the `Simulator` dictates order of operations as follows:
1. Stream incoming `IMarketEvent` entries (Merge sorted OHLCV/Factor records).
2. For each new `Timestamp` jump:
   - Calculate potential unfulfilled `Order` records dynamically via Broker.
   - Run the execution strategy (`DecisionCycle`).
   - Trigger `IBroker.ProcessOrders` against current `MarketState`.
   - Execute limit / market boundaries.
   - Record active portfolio cash/equity states sequentially through `IRecorder`.

## ⚙️ Configuration Example

Execution and orchestration can be mapped directly onto a `.yaml` or JSON file schema specifying the execution definitions:

```yaml
name: sample

instruments:
  - MSFT

factors:
  - symbol: MSFT
    name: eps
  
startDate: 2000-01-01
endDate: 2030-01-01

strategy:
  type: BBB
  parameters:
    period: 5000
    sideperiod: 5000
    stdm: 2.5
    source: close
```