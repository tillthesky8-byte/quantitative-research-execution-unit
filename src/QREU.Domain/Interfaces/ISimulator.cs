using Domain.Models;

namespace Domain.Interfaces;
public interface ISimulator
{
    Task<SimulationResult> Run(IStreamer streamer);
}
