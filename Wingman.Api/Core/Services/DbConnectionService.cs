using Npgsql;
using System.Data;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Services;

public class DbConnectionService : IDbConnectionService
{
    private readonly IConfiguration _configuration;
    private readonly string _connectionString;

    public DbConnectionService(IConfiguration configuration)
    {
        _configuration = configuration;
        _connectionString = _configuration.GetConnectionString("PostgreSQLConnection")!;
    }

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);
}
