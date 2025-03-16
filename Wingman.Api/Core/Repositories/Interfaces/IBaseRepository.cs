using Wingman.Api.Core.Models;

namespace Wingman.Api.Core.Repositories.Interfaces;

public interface IBaseRepository<T> where T : BaseModel
{
    public Task<T?> GetByIdAsync(Guid id);
    public Task<Guid> CreateAsync(T model);
    public Task<bool> UpdateAsync(T model);
    public Task<bool> DeleteByIdAsync(Guid id);
}
