using Wingman.Api.Core.DTOs;
using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Services;

public abstract class BaseService<T> : IBaseService<T>
{
    private readonly IBaseRepository<T> _repo;
    private readonly string _modelName;

    public BaseService(IBaseRepository<T> repo)
    {
        _repo = repo;
        _modelName = $"{typeof(T).Name}";
    }

    public async Task<ApiResponseDto<Guid>> CreateAsync(T model)
    {
        Guid id = await _repo.CreateAsync(model);

        return new()
        {
            StatusCode = StatusCodes.Status201Created,
            Data = id,
            Message = $"{_modelName} successfully created."
        };
    }

    public async Task<ApiResponseDto<T>> GetByIdAsync(Guid id)
    {
        T? model = await _repo.GetByIdAsync(id);

        if (model is not null)
        {
            return new()
            {
                StatusCode = StatusCodes.Status200OK,
                Data = model,
            };
        }

        return new()
        {
            StatusCode = StatusCodes.Status404NotFound,
            Message = $"No {_modelName} was found for the specified ID."
        };
    }
}
