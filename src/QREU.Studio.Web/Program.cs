
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


        var app = builder.Build();

        // curl -X GET http://localhost:9999/runs
        app.MapGet("/runs", async (IRunRepository repo) =>
        {
            var runs = await repo.GetRunsAsync();
            return Results.Ok(runs);
        });

        app.Run();
    }
}