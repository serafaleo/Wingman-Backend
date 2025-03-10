using Wingman.Api.Core.DTOs;

namespace Wingman.Api.Core.Services.Interfaces;

public interface IBaseService<T>
{
    public Task<ApiResponseDto<List<T>>> GetAllAsync(Guid userId, int page, int pageSize);
    public Task<ApiResponseDto<T>> GetByIdAsync(Guid id);
    public Task<ApiResponseDto<Guid>> CreateAsync(T model, Guid userId);
    public Task<ApiResponseDto<object>> UpdateAsync(T model, Guid userId);
    public Task<ApiResponseDto<object>> DeleteByIdAsync(Guid id);
}
