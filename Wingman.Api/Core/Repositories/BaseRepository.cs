using Dapper;
using System.Data;
using System.Reflection;
using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Repositories;

public class BaseRepository<T> : IBaseRepository<T>
{
    protected readonly IDbConnectionService _db;
    protected readonly string _tableName;

    public BaseRepository(IDbConnectionService db)
    {
        _db = db;
        _tableName = $"\"{typeof(T).Name}s\"";
    }

    public async Task<Guid> CreateAsync(T model)
    {
        // NOTE(serafa.leo): Removing the Id property because we want the database to assign Id automatically.
        IEnumerable<PropertyInfo> properties = typeof(T).GetProperties().Where(p => p.Name != "Id");

        string columnNames = string.Join(", ", properties.Select(p => BuildColumnName(p.Name)));
        string parameterNames = string.Join(", ", properties.Select(p => BuildParameterName(p.Name)));

        string query =
$@"INSERT INTO {_tableName} ({columnNames})
VALUES ({parameterNames})
RETURNING ""Id"";";

        using IDbConnection connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(query, model);
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var queryParams = new { Id = id };

        string idParamName = $"@{nameof(queryParams.Id)}";

        string query = $"SELECT * FROM {_tableName} WHERE \"Id\" = {idParamName}";

        using IDbConnection connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(query, queryParams);
    }

    protected static string BuildColumnName(string fieldName)
    {
        return $"\"{fieldName}\"";
    }

    protected static string BuildParameterName(string fieldName)
    {
        return $"@{fieldName}";
    }
}
