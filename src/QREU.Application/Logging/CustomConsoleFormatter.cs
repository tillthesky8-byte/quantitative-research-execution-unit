using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Logging.Console;
using Domain.Other;

namespace Application.Logging;

public sealed class CustomConsoleFormatter : ConsoleFormatter
{
    public CustomConsoleFormatter() : base("custom") {}

    public override void Write<TState>(in LogEntry<TState> logEntry, IExternalScopeProvider? scopeProvider, TextWriter textWriter)
    {
        var time     = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var level    = logEntry.LogLevel.ToString().ToUpper();
        var category = logEntry.Category;
        var message  = logEntry.Formatter?.Invoke(logEntry.State, logEntry.Exception);

        var levelColor = logEntry.LogLevel switch
        {
            LogLevel.Trace       => ConsoleColors.Red,
            LogLevel.Debug       => ConsoleColors.Blue,
            LogLevel.Information => ConsoleColors.Green,
            LogLevel.Warning     => ConsoleColors.Yellow,
            LogLevel.Error       => ConsoleColors.Red,
            LogLevel.Critical    => ConsoleColors.Red,
            _ => ConsoleColors.Reset
        };

        var levelString = logEntry.LogLevel switch
        {
            LogLevel.Trace => "TRCE",
            LogLevel.Debug => "DBUG",
            LogLevel.Information => "INFO",
            LogLevel.Warning => "WARN",
            LogLevel.Error => "ERRO",
            LogLevel.Critical => "CRIT",
            _ => level
        };

        textWriter.Write($"[{time}] ");
        textWriter.Write($"{ConsoleColors.CategoryBackgroundColor}{levelColor}{levelString}{ConsoleColors.Reset} ");

        textWriter.Write($"{ConsoleColors.CategoryBackgroundColor}{ConsoleColors.CategoryForegroundColor}{category, -40}{ConsoleColors.Reset}| ");
        textWriter.Write($"{message}");

        if (logEntry.Exception is not null)
            textWriter.Write($" | Exception: {logEntry.Exception}");

        textWriter.WriteLine();
    }
}