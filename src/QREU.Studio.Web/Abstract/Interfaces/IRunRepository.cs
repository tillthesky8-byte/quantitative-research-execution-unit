using Studio.Web.Models;

namespace Studio.Web.Interfaces;

public interface IRunRepository
{
    Task<IEnumerable<Run>> GetRunsAsync();
}

