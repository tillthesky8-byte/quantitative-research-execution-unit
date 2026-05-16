using DuckDB.NET.Data;
using Studio.Web.Interfaces;
using Studio.Web.Models;
namespace Studio.Web.Repositories;

public class DbConnectionFactory : IDbConnectionFactory
{
    private readonly ILogger<DbConnectionFactory> _logger;
    private readonly string _connectionString;
    public DbConnectionFactory(ILogger<DbConnectionFactory> logger, IConfiguration configuration)
    {
        _logger = logger;
        _connectionString = configuration.GetConnectionString("DuckDb") ?? throw new InvalidOperationException("Connection string 'DuckDb' not found.");
    }
    public Task<DuckDBConnection> CreateConnectionAsync()
    {
        var connection = new DuckDBConnection(_connectionString);
        return Task.FromResult(connection);
    }

}