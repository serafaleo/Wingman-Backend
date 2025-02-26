using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Features.Auth.Models;

namespace Wingman.Api.Features.Auth.Repositories.Interfaces;

public interface IUsersRepository : IBaseRepository<User>
{
    public Task<User?> GetUserByEmailAsync(string email);
    public Task UpdateRefreshTokenAsync(User user);
}
