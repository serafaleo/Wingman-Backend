using Wingman.Api.Core.DTOs;

namespace Wingman.Api.Core.Services.Interfaces;

public interface IBaseService<T>
{
    public Task<ApiResponseDto<Guid>> CreateAsync(T model);
    public Task<ApiResponseDto<T>> GetByIdAsync(Guid id);
}
