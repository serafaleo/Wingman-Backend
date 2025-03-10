namespace Wingman.Api.Core.Repositories.Interfaces;

public interface IBaseRepository<T>
{
    public Task<List<T>> GetAllAsync(Guid userId, int page, int pageSize);
    public Task<T?> GetByIdAsync(Guid id);
    public Task<Guid> CreateAsync(T model);
    public Task<bool> UpdateAsync(T model);
    public Task<bool> DeleteByIdAsync(Guid id);
}
