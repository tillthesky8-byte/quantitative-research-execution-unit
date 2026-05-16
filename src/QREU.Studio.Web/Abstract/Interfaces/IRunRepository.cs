using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface IRunRepository
{
    Task<IEnumerable<RawRun>> GetRunsAsync();
    Task<RawRun> GetRunAsync(Guid runId);
}

