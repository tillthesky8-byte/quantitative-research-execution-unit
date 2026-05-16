using DuckDB.NET.Data;

namespace Studio.Web.Interfaces;

public interface IDbConnectionFactory
{
    Task<DuckDBConnection> CreateConnectionAsync();
}