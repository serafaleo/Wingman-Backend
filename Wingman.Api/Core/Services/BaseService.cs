using System.Reflection;
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

    public async Task<ApiResponseDto<List<T>>> GetAllAsync(Guid userId, int page, int pageSize)
    {
        List<T> entities = await _repo.GetAllAsync(userId, page, pageSize);

        return new()
        {
            StatusCode = StatusCodes.Status200OK,
            Data = entities,
            Message = $"Retrieved {entities.Count} {_modelName}s."
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
            Message = $"No {_modelName} with Id {id} was found."
        };
    }

    public async Task<ApiResponseDto<Guid>> CreateAsync(T model, Guid userId)
    {
        FieldInfo? userIdField = typeof(T).GetField("UserId");

        if (userIdField is not null)
        {
            userIdField.SetValue(model, userId);
        }

        Guid id = await _repo.CreateAsync(model);

        return new()
        {
            StatusCode = StatusCodes.Status201Created,
            Data = id,
            Message = $"{_modelName} successfully created."
        };
    }

    public async Task<ApiResponseDto<object>> UpdateAsync(T model, Guid userId)
    {
        FieldInfo? userIdField = typeof(T).GetField("UserId");

        if (userIdField is not null)
        {
            if ((Guid)userIdField.GetValue(model)! != userId)
            {
                return new()
                {
                    StatusCode = StatusCodes.Status400BadRequest,
                    Message = $"The {_modelName} requested for update does not belong to the logged user."
                };
            }
        }

        bool result = await _repo.UpdateAsync(model);

        if (!result)
        {
            return new()
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"{_modelName} not found for update."
            };
        }

        return new()
        {
            StatusCode = StatusCodes.Status200OK,
            Message = $"{_modelName} successfully updated."
        };
    }

    public async Task<ApiResponseDto<object>> DeleteByIdAsync(Guid id)
    {
        bool result = await _repo.DeleteByIdAsync(id);

        if (!result)
        {
            return new()
            {
                StatusCode = StatusCodes.Status404NotFound,
                Message = $"{_modelName} not found for to delete."
            };
        }

        return new()
        {
            StatusCode = StatusCodes.Status200OK,
            Message = $"{_modelName} successfully deleted."
        };
    }
}
