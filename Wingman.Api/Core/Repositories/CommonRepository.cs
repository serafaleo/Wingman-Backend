using Dapper;
using System.Data;
using Wingman.Api.Core.Models;
using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Repositories;

public abstract class CommonRepository<T> : BaseRepository<T>, ICommonRepository<T> where T : CommonModel
{
    protected readonly string _userIdColumnName;

    public CommonRepository(IDbConnectionService db) : base(db)
    {
        T tempModel;

        _userIdColumnName = BuildColumnName(nameof(tempModel.UserId));
    }

    public async Task<List<T>> GetAllAsync(Guid userId, int page, int pageSize)
    {
        int offset = (page - 1) * pageSize;

        var queryParams = new { UserId = userId, PageSize = pageSize, Offset = offset };

        string userIdParamName = BuildParameterName(nameof(queryParams.UserId));
        string pageSizeParamName = BuildParameterName(nameof(queryParams.PageSize));
        string offsetParamName = BuildParameterName(nameof(queryParams.Offset));

        string query = $@"SELECT * FROM {_tableName}
                          WHERE {_userIdColumnName} = {userIdParamName}
                          LIMIT {pageSizeParamName} OFFSET {offsetParamName}";

        using IDbConnection connection = _db.CreateConnection();
        return (await connection.QueryAsync<T>(query, queryParams)).ToList();
    }
}
