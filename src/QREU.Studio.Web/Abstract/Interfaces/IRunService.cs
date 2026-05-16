using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface IRunService
{
    Task<IEnumerable<Run>> GetRunsAsync();
    Task<Run> GetRunAsync(Guid runId);
}