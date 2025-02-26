namespace Wingman.Api.Core.Repositories.Interfaces;

public interface IBaseRepository<T>
{
    public Task<Guid> CreateAsync(T model);
    public Task<T?> GetByIdAsync(Guid id);
}
