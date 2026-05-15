using Domain.Models;
namespace Core.Interfaces;

public interface IRecorder
{
    void Record(long timestamp, decimal equity);
    void AppendTrades(IEnumerable<TradeRecord> trades);
    void AppendRealizedPnlEvents(IEnumerable<RealizedPnlEvent> events);
    SimulationResult BuildResult();
}