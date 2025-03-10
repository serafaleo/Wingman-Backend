using Wingman.Api.Core.DTOs;

namespace Wingman.Api.Core.Services.Interfaces;

public interface IBaseService<T>
{
    public Task<ApiResponseDto<List<T>>> GetAllAsync(int page, int pageSize, Guid contextUserId);
    public Task<ApiResponseDto<T>> GetByIdAsync(Guid id, Guid contextUserId);
    public Task<ApiResponseDto<Guid>> CreateAsync(T model, Guid contextUserId);
    public Task<ApiResponseDto<object>> UpdateAsync(Guid id, T model, Guid contextUserId);
    public Task<ApiResponseDto<object>> DeleteByIdAsync(Guid id, Guid contextUserId);
}
