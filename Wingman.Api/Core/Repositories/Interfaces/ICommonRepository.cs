using Wingman.Api.Core.Models;

namespace Wingman.Api.Core.Repositories.Interfaces;

public interface ICommonRepository<T> : IBaseRepository<T> where T : CommonModel
{
    public Task<List<T>> GetAllAsync(Guid userId, int page, int pageSize);
}
