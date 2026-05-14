using System.CommandLine;
using Application.Logging;
using Application.Models;
using Domain.Other;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Application;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        var builder     = Host.CreateApplicationBuilder(args);
        var appSettings = new AppSettings(builder.Configuration);
    
        builder.Services.AddSingleton(appSettings);
        builder.Services.AddLogging(config =>
        {
            config.ClearProviders();
            config.AddConsole(options => options.FormatterName = "custom");
            config.Services.AddSingleton<ConsoleFormatter, CustomConsoleFormatter>();
        });

        var app = builder.Build();
        
        var logger        = app.Services.GetRequiredService<ILogger<Program>>();
        var loggerFactory = app.Services.GetRequiredService<ILoggerFactory>();

        var resolvedAppSettings = app.Services.GetRequiredService<AppSettings>();
        logger.LogInformation(LogMessages.AppSettingsResolved, resolvedAppSettings.ConnectionString, resolvedAppSettings.ConfigurationRoot);
        
        var rootCommand = new Root(appSettings, loggerFactory.CreateLogger<Root>(), loggerFactory);

        return await rootCommand.Parse(args).InvokeAsync();
    }
}