
namespace Domain.Other;
public static class LogMessages
{
    public const string AppSettingsResolved = "Resolved AppSettings: ConnectionString={ConnectionString}, ConfigurationRoot={ConfigurationRoot}";
    public const string RunConfigurationResolved = "Resolved RunConfiguration: Instruments=[{Instruments}], Factors=[{Factors}], StartDate={StartDate}, EndDate={EndDate}, StrategyType={StrategyType}, StrategyParameters={StrategyParameters}";
    public const string OhlcvQueryResolved = "Resolved Queries: OhlcvQuery={OhlcvQuery}";
    public const string FactorQueryResolved = "Resolved Queries: FactorQuery={FactorQuery}";
    public const string ConnectionOpened = "Database connection opened. ConnectionString={ConnectionString}, Database={Database}, DataSource={DataSource}, State={State}, ServerVersion={ServerVersion} ";
    public const string PositionOpened = "Position opened: Symbol={Symbol}, Quantity={Quantity}, AverageEntryPrice={AverageEntryPrice}";
    public const string PositionClosed = "Position closed: Symbol={Symbol}, Quantity={Quantity}, AverageEntryPrice={AverageEntryPrice}, RealizedPnl={RealizedPnl}, Commission={Commission}, ExitPrice={ExitPrice}";
    public const string PortfolioSummaryAtTimestamp = $"{ConsoleColors.Cyan}{{Timestamp}}{ConsoleColors.Reset} - PS: Cash={{Cash}}, Equity={{Equity}}, Positions=[{{Positions}}], RPE Count={{RpeCount}}";
}
