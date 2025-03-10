using Dapper;
using System.Data;
using System.Reflection;
using System.Text;
using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Repositories;

public abstract class BaseRepository<T> : IBaseRepository<T>
{
    protected readonly IDbConnectionService _db;
    protected readonly string _tableName;

    public BaseRepository(IDbConnectionService db)
    {
        _db = db;
        _tableName = $"\"{typeof(T).Name}s\"";
    }

    public async Task<List<T>> GetAllAsync(Guid userId, int page, int pageSize)
    {
        int offset = (page - 1) * pageSize;

        var queryParams = new { UserId = userId, PageSize = pageSize, Offset = offset };

        string userIdParamName = BuildParameterName(nameof(queryParams.UserId));
        string pageSizeParamName = BuildParameterName(nameof(queryParams.PageSize));
        string offsetParamName = BuildParameterName(nameof(queryParams.Offset));

        string query = $@"SELECT * FROM {_tableName}
                          LIMIT {pageSizeParamName}
                          OFFSET {offsetParamName}
                          WHERE ""UserId"" = {userIdParamName}";

        using IDbConnection connection = _db.CreateConnection();
        return (await connection.QueryAsync<T>(query, queryParams)).ToList();
    }

    public async Task<T?> GetByIdAsync(Guid id)
    {
        var queryParams = new { Id = id };

        string idParamName = BuildParameterName(nameof(queryParams.Id));

        string query = $"SELECT * FROM {_tableName} WHERE \"Id\" = {idParamName}";

        using IDbConnection connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<T>(query, queryParams);
    }

    public async Task<Guid> CreateAsync(T model)
    {
        IEnumerable<PropertyInfo> properties = GetModelPropertiesExceptPrimaryKey(model);

        string columnNames = string.Join(", ", properties.Select(p => BuildColumnName(p.Name)));
        string parameterNames = string.Join(", ", properties.Select(p => BuildParameterName(p.Name)));

        string query = $@"INSERT INTO {_tableName} ({columnNames})
                          VALUES ({parameterNames})
                          RETURNING ""Id"";";

        using IDbConnection connection = _db.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(query, model);
    }

    public async Task<bool> UpdateAsync(T model)
    {
        IEnumerable<PropertyInfo> properties = GetModelPropertiesExceptPrimaryKey(model);

        StringBuilder setClause = new StringBuilder();

        foreach (PropertyInfo property in properties)
        {
            setClause.Append($"{BuildColumnName(property.Name)} = {BuildParameterName(property.Name)}, ");
        }

        setClause.Length -= 2; // Remove last comma

        string query = $@"UPDATE {_tableName}
                          SET {setClause}
                          WHERE ""Id"" = @Id";

        using IDbConnection connection = _db.CreateConnection();

        int rowsAffected = await connection.ExecuteAsync(query, model);
        return rowsAffected > 0;
    }

    public async Task<bool> DeleteByIdAsync(Guid id)
    {
        var queryParams = new { Id = id };

        string idParamName = BuildParameterName(nameof(queryParams.Id));

        string query = $"DELETE FROM {_tableName} WHERE \"Id\" = {idParamName}";

        IDbConnection connection = _db.CreateConnection();

        int rowsAffected = await connection.ExecuteAsync(query, queryParams);
        return rowsAffected > 0;
    }

    protected static string BuildColumnName(string fieldName)
    {
        return $"\"{fieldName}\"";
    }

    protected static string BuildParameterName(string fieldName)
    {
        return $"@{fieldName}";
    }

    private IEnumerable<PropertyInfo> GetModelPropertiesExceptPrimaryKey(T model)
    {
        // NOTE(serafa.leo): Removing the Id property because we want the database to assign Id automatically.
        return typeof(T).GetProperties().Where(p => p.Name != "Id");
    }
}
