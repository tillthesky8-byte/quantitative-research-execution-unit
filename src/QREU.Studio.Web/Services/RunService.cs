using Studio.Web.Interfaces;
using Studio.Web.Models;

namespace Studio.Web.Services;

public class RunService : IRunService
{
    private readonly ILogger<RunService> _logger;   

    private readonly IRunRepository _runRepository;

    public RunService(ILogger<RunService> logger, IRunRepository runRepository)
    {
        _logger = logger;
        _runRepository = runRepository;
    }

    public async Task<IEnumerable<Run>> GetRunsAsync()
    {
        var rawRuns = await _runRepository.GetRunsAsync();
        var runs = rawRuns.Select(rawRun =>
        {
            var config = rawRun.ConfigJson;
            return new Run
            (
                Id           : rawRun.Id,
                RanAt        : rawRun.RanAt,
                StrategyName : rawRun.StrategyName,
                Symbols      : config.Dataset.Instruments.Select(i => i.Symbol).ToArray(),
                StartDate    : config.Dataset.StartDate,
                EndDate      : config.Dataset.EndDate
            );
        });
        return runs;
    }

    public async Task<Run> GetRunAsync(Guid runId)
    {
        var rawRun = await _runRepository.GetRunAsync(runId);
        if (rawRun == null)
        {
            return null!;
        }

        var config = rawRun.ConfigJson;
        var run = new Run
        (
            Id           : rawRun.Id,
            RanAt        : rawRun.RanAt,
            StrategyName : rawRun.StrategyName,
            Symbols      : config.Dataset.Instruments.Select(i => i.Symbol).ToArray(),
            StartDate    : config.Dataset.StartDate,
            EndDate      : config.Dataset.EndDate
        );
        return run;
    }
    
}