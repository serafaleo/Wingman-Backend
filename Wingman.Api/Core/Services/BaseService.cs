using System.Reflection;
using Wingman.Api.Core.DTOs;
using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Services;

public abstract class BaseService<T> : IBaseService<T>
{
    private readonly IBaseRepository<T> _repo;
    private readonly string _modelName;
    private readonly string _defaultNotFoundMessage;
    private const string _userIdFieldName = "UserId";

    public BaseService(IBaseRepository<T> repo)
    {
        _repo = repo;
        _modelName = $"{typeof(T).Name}";
        _defaultNotFoundMessage = $"{_modelName} not found in the server.";
    }

    public async Task<ApiResponseDto<List<T>>> GetAllAsync(int page, int pageSize, Guid contextUserId)
    {
        List<T> entities = await _repo.GetAllAsync(contextUserId, page, pageSize);

        return new()
        {
            StatusCode = StatusCodes.Status200OK,
            Data = entities
        };
    }

    public async Task<ApiResponseDto<T>> GetByIdAsync(Guid id, Guid contextUserId)
    {
        T? model = await _repo.GetByIdAsync(id);

        if (model is not null)
        {
            if (!UserHasPermission(model, contextUserId))
            {
                return new()
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            return new()
            {
                StatusCode = StatusCodes.Status200OK,
                Data = model,
            };
        }

        return new()
        {
            StatusCode = StatusCodes.Status404NotFound,
            Message = _defaultNotFoundMessage
        };
    }

    public async Task<ApiResponseDto<Guid>> CreateAsync(T model, Guid contextUserId)
    {
        SetUserIdValueToModel(model, contextUserId);

        Guid id = await _repo.CreateAsync(model);

        return new()
        {
            StatusCode = StatusCodes.Status201Created,
            Data = id,
        };
    }

    public async Task<ApiResponseDto<object>> UpdateAsync(Guid id, T model, Guid contextUserId)
    {
        T? modelInDataBase = await _repo.GetByIdAsync(id);

        if (modelInDataBase is not null)
        {
            if (!UserHasPermission(modelInDataBase, contextUserId))
            {
                return new()
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            await _repo.UpdateAsync(model);

            return new()
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        return new()
        {
            StatusCode = StatusCodes.Status404NotFound,
            Message = _defaultNotFoundMessage
        };
    }

    public async Task<ApiResponseDto<object>> DeleteByIdAsync(Guid id, Guid contextUserId)
    {
        T? modelInDataBase = await _repo.GetByIdAsync(id);

        if (modelInDataBase is not null)
        {
            if (!UserHasPermission(modelInDataBase, contextUserId))
            {
                return new()
                {
                    StatusCode = StatusCodes.Status403Forbidden
                };
            }

            await _repo.DeleteByIdAsync(id);

            return new()
            {
                StatusCode = StatusCodes.Status200OK
            };
        }

        return new()
        {
            StatusCode = StatusCodes.Status404NotFound,
            Message = _defaultNotFoundMessage
        };
    }

    private bool UserHasPermission(T modelInDataBase, Guid contextUserId)
    {
        Guid? userIdInDataBase = GetUserIdValueFromModel(modelInDataBase);

        if (userIdInDataBase is not null && userIdInDataBase != contextUserId)
        {
            return false;
        }

        return true;
    }

    private Guid? GetUserIdValueFromModel(T model)
    {
        FieldInfo? userIdField = typeof(T).GetField(_userIdFieldName);

        if (userIdField is not null)
        {
            return (Guid?)userIdField.GetValue(model);
        }

        return null;
    }

    private void SetUserIdValueToModel(T model, Guid userId)
    {
        FieldInfo? userIdField = typeof(T).GetField(_userIdFieldName);

        if (userIdField is not null)
        {
            userIdField.SetValue(model, userId);
        }
    }
}
