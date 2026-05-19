
using Studio.Web.Interfaces;
using Studio.Web.Repositories;
using Studio.Web.Services;

namespace Studio.Web;

internal class Program
{
    private static void Main(string[] args)
    {

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<IDbConnectionFactory, DbConnectionFactory>();

        builder.Services.AddScoped<IRunRepository, RunRepository>();
        builder.Services.AddScoped<ISeriesRepository, SeriesRepository>();
        builder.Services.AddScoped<ISeriesChunkRepository, SeriesChunkRepository>();

        builder.Services.AddScoped<IRunService, RunService>();
        builder.Services.AddScoped<ISeriesService, SeriesService>();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        var app = builder.Build();

        var logger = app.Services.GetRequiredService<ILogger<Program>>();

        app.UseCors("AllowAll");


        // curl -X GET http://localhost:9999/api/runs
        app.MapGet("/api/runs", async (IRunService service) =>
        {
            logger.LogInformation("Received request for runs");
            var runs = await service.GetRunsAsync();
            return Results.Ok(runs);
        });

        app.MapGet("/api/runs/{runId:guid}", async (IRunService service, Guid runId) =>
        {
            logger.LogInformation("Received request for run details: RunId={RunId}", runId);
            var run = await service.GetRunAsync(runId);
            if (run == null)
            {
                return Results.NotFound();
            }
            return Results.Ok(run);
        });

        // curl -X GET http://localhost:9999/api/series/backward-chunk?runId=84c3044a-7a79-45f0-b9ed-e008325f47be&symbol=MSFT&timeframe=1d&from=170406720000&chunkSize=100
        app.MapGet("/api/series/backward-chunk", async (ISeriesService service, Guid runId, string symbol, string timeframe, long from, int chunkSize) =>
        {
            logger.LogInformation("Received request for backward series chunk bundle: RunId={RunId}, Symbol={Symbol}, Timeframe={Timeframe}, From={From}, ChunkSize={ChunkSize}", runId, symbol, timeframe, from, chunkSize);
            var seriesBundle = await service.GetBackwardSeriesChunkBundleAsync(runId, symbol, timeframe, from, chunkSize);
            return Results.Ok(seriesBundle);
        });


        // curl -X GET http://localhost:9999/api/series/forward-chunk?runId=84c3044a-7a79-45f0-b9ed-e008325f47be&symbol=MSFT&timeframe=1d&to=1704067200&chunkSize=100
        app.MapGet("/api/series/forward-chunk", async (ISeriesService service, Guid runId, string symbol, string timeframe, long to, int chunkSize) =>
        {
            logger.LogInformation("Received request for forward series chunk bundle: RunId={RunId}, Symbol={Symbol}, Timeframe={Timeframe}, To={To}, ChunkSize={ChunkSize}", runId, symbol, timeframe, to, chunkSize);
            var seriesBundle = await service.GetForwardSeriesChunkBundleAsync(runId, symbol, timeframe, to, chunkSize);
            return Results.Ok(seriesBundle);
        });

        // curl -X GET "http://localhost:9999/api/series?runId=379d63a1-fd9a-43ad-8f48-5e03ddd76707&symbol=MSFT&timeframe=1d&from=2024-01-01&to=2024-01-31"
        app.MapGet("/api/series", async (ISeriesService service, Guid runId, string symbol, string timeframe, string from, string to) =>
        {
            logger.LogInformation("Received request for series bundle: RunId={RunId}, Symbol={Symbol}, Timeframe={Timeframe}, From={From}, To={To}", runId, symbol, timeframe, from, to);
            var seriesBundle = await service.GetSeriesBundleAsync(runId, symbol, timeframe, DateTime.ParseExact(from, "yyyy-MM-dd", null), DateTime.ParseExact(to, "yyyy-MM-dd", null));
            return Results.Ok(seriesBundle);
        });

        // curl -X GET "http://localhost:9999/api/trades?runId=379d63a1-fd9a-43ad-8f48-5e03ddd76707&from=2024-01-01&to=2024-01-31&page=1&pageSize=100"
        app.MapGet("/api/trades", async (ISeriesRepository repo, Guid runId, string from, string to, int page = 1, int pageSize = 100) =>
        {
            var trades = await repo.GetTradesAsync(runId, DateTime.ParseExact(from, "yyyy-MM-dd", null), DateTime.ParseExact(to, "yyyy-MM-dd", null), page, pageSize);
            return Results.Ok(trades);
        });

        app.Run();
    }
}