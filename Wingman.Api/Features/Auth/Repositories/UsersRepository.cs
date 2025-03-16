using Dapper;
using System.Data;
using Wingman.Api.Core.Repositories;
using Wingman.Api.Core.Services.Interfaces;
using Wingman.Api.Features.Auth.Models;
using Wingman.Api.Features.Auth.Repositories.Interfaces;

namespace Wingman.Api.Features.Auth.Repositories;

public class UsersRepository(IDbConnectionService db) : BaseRepository<User>(db), IUsersRepository
{
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        var queryParams = new { Email = email };

        string emailColumnName = BuildColumnName(nameof(User.Email));
        string emailParamName = BuildParameterName(nameof(queryParams.Email));

        string query = $"SELECT * FROM {_tableName} WHERE {emailColumnName} = {emailParamName}";

        using IDbConnection connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(query, queryParams);
    }

    public async Task UpdateRefreshTokenAsync(User user)
    {
        string refreshTokenColumnName = BuildColumnName(nameof(User.RefreshToken));
        string refreshTokenParamName = BuildParameterName(nameof(User.RefreshToken));
        string refreshTokenExpirationDateColumnName = BuildColumnName(nameof(User.RefreshTokenExpirationDateTimeUTC));
        string refreshTokenExpirationDateParamName = BuildParameterName(nameof(User.RefreshTokenExpirationDateTimeUTC));

        string query = $@"UPDATE {_tableName}
                          SET {refreshTokenColumnName} = {refreshTokenParamName},
                              {refreshTokenExpirationDateColumnName} = {refreshTokenExpirationDateParamName}
                          WHERE {_idColumnName} = {_idParamName}";

        using IDbConnection connection = _db.CreateConnection();
        await connection.ExecuteAsync(query, user);
    }
}
