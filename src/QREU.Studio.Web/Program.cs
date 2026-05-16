
using Studio.Web.Interfaces;
using Studio.Web.Repositories;

namespace Studio.Web;

internal class Program
{
    private static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        builder.Services.AddScoped<IRunRepository, RunRepository>();
        builder.Services.AddScoped<ISeriesRepository, SeriesRepository>();


        var app = builder.Build();

        // curl -X GET http://localhost:9999/runs
        app.MapGet("/runs", async (IRunRepository repo) =>
        {
            var runs = await repo.GetRunsAsync();
            return Results.Ok(runs);
        });

        // curl -X GET "http://localhost:9999/ohlc?symbol=MSFT&from=2024-01-01&to=2024-01-31"
        app.MapGet("/ohlc", async (ISeriesRepository repo, string symbol, string from, string to) =>
        {
            var ohlcList = await repo.GetOhlcAsync(symbol, DateTime.ParseExact(from, "yyyy-MM-dd", null), DateTime.ParseExact(to, "yyyy-MM-dd", null));
            return Results.Ok(ohlcList);
        });

        // curl -X GET "http://localhost:9999/equity?runId=379d63a1-fd9a-43ad-8f48-5e03ddd76707&from=2024-01-01&to=2024-01-31"
        app.MapGet("/equity", async (ISeriesRepository repo, Guid runId, string from, string to) =>
        {
            var equityPoints = await repo.GetEquityCurveAsync(runId, DateTime.ParseExact(from, "yyyy-MM-dd", null), DateTime.ParseExact(to, "yyyy-MM-dd", null));
            return Results.Ok(equityPoints);
        });

        // curl -X GET "http://localhost:9999/trades?runId=379d63a1-fd9a-43ad-8f48-5e03ddd76707&from=2024-01-01&to=2024-01-31&page=1&pageSize=100"
        app.MapGet("/trades", async (ISeriesRepository repo, Guid runId, string from, string to, int page = 1, int pageSize = 100) =>
        {
            var trades = await repo.GetTradesAsync(runId, DateTime.ParseExact(from, "yyyy-MM-dd", null), DateTime.ParseExact(to, "yyyy-MM-dd", null), page, pageSize);
            return Results.Ok(trades);
        });

        app.Run();
    }
}