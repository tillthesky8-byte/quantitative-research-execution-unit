using Domain.Models;
using Microsoft.Extensions.Logging;

namespace Application.Runners;

public sealed class RootRunner
{
    private readonly RunConfiguration _configuration;
    private readonly ILogger<RootRunner>? _logger;
    private readonly ILoggerFactory _loggerFactory;

    public RootRunner(RunConfiguration configuration, ILogger<RootRunner>? logger = null, ILoggerFactory? loggerFactory = null)
    {
        _configuration = configuration;
        _logger = logger;
        _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
    }
    public void Run()
    {
        //====================================================
        // BUILD COMPONENTS
        //====================================================


        //====================================================
        // ROOT EXECUTION
        //====================================================


        //====================================================
        // REPORTING
        //====================================================
    }
}