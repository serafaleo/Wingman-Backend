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

        string emailColumnName = nameof(User.Email);
        string emailParamName = $"@{nameof(queryParams.Email)}";

        string query = $"SELECT * FROM {_tableName} WHERE {emailColumnName} = {emailParamName}";

        using IDbConnection connection = _db.CreateConnection();
        return await connection.QuerySingleOrDefaultAsync<User>(query, queryParams);
    }

    public async Task UpdateRefreshTokenAsync(User user)
    {
        string refreshTokenColumnName = nameof(User.RefreshToken);
        string refreshTokenParamName = $"@{refreshTokenColumnName}";
        string refreshTokenExpirationDateColumnName = nameof(User.RefreshTokenExpirationDateTimeUTC);
        string refreshTokenExpirationDateParamName = $"@{refreshTokenExpirationDateColumnName}";
        string userIdColumnName = nameof(User.Id);
        string userIdParamName = $"@{userIdColumnName}";

        string query =
$@"UPDATE {_tableName}
   SET {refreshTokenColumnName} = {refreshTokenParamName},
       {refreshTokenExpirationDateColumnName} = {refreshTokenExpirationDateParamName}
   WHERE {userIdColumnName} = {userIdParamName}";

        using IDbConnection connection = _db.CreateConnection();
        await connection.ExecuteAsync(query, user);
    }
}
