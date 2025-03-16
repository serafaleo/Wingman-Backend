using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Wingman.Api.Core.Helpers.ExtensionMethods;
using Wingman.Api.Core.Models;
using Wingman.Api.Core.Repositories.Interfaces;
using Wingman.Api.Core.Services.Interfaces;

namespace Wingman.Api.Core.Services;

public abstract class CommonService<T>(ICommonRepository<T> repo) : BaseService<T>, ICommonService<T> where T : CommonModel
{
    private readonly ICommonRepository<T> _repo = repo;

    public async Task<Either<ProblemDetails, List<T>>> GetAllAsync(int page, int pageSize, Guid contextUserId)
    {
        if (page < 1 || pageSize < 1)
        {
            return new ProblemDetails().BadRequest($"Failed to get {_modelName}s.", "Invalid pagination parameters.");
        }

        return await _repo.GetAllAsync(contextUserId, page, pageSize);
    }

    public async Task<Either<ProblemDetails, T>> GetByIdAsync(Guid id, Guid contextUserId)
    {
        Either<ProblemDetails, T> validation = await ValidateModelInDatabase(id, "get", contextUserId);

        if (validation.IsLeft)
        {
            return validation.LeftAsEnumerable().First();
        }

        return validation.RightAsEnumerable().First();
    }

    public async Task<Either<ProblemDetails, Unit>> CreateAsync(T model, Guid contextUserId)
    {
        model.UserId = contextUserId;
        model.Id = await _repo.CreateAsync(model);
        return new Unit();
    }

    public async Task<Either<ProblemDetails, Unit>> UpdateAsync(Guid id, T model, Guid contextUserId)
    {
        const string action = "update";

        if (model.Id != default && model.Id != id)
        {
            return new ProblemDetails().BadRequest(BuildDefaultErrorTitle(id, action), "Body object ID and route ID are different.");
        }

        Either<ProblemDetails, T> validation = await ValidateModelInDatabase(id, action, contextUserId);

        if (validation.IsLeft)
        {
            return validation.LeftAsEnumerable().First();
        }

        if (model.UserId != default && model.UserId != contextUserId)
        {
            return new ProblemDetails().BadRequest(BuildDefaultErrorTitle(id, action), "Body object UserID was changed, which is not permitted.");
        }

        model.UserId = contextUserId;
        await _repo.UpdateAsync(model);
        return new Unit();
    }

    public async Task<Either<ProblemDetails, Unit>> DeleteByIdAsync(Guid id, Guid contextUserId)
    {
        Either<ProblemDetails, T> validation = await ValidateModelInDatabase(id, "delete", contextUserId);

        if (validation.IsLeft)
        {
            return validation.LeftAsEnumerable().First();
        }

        await _repo.DeleteByIdAsync(id);
        return new Unit();
    }

    private async Task<Either<ProblemDetails, T>> ValidateModelInDatabase(Guid id, string action, Guid contextUserId)
    {
        T? modelInDataBase = await _repo.GetByIdAsync(id);
        string defaultErrorTitle = BuildDefaultErrorTitle(id, action);

        if (modelInDataBase is null)
        {
            return new ProblemDetails().DefaultNotFound(defaultErrorTitle, _modelName);
        }

        if (!UserHasPermission(modelInDataBase, contextUserId))
        {
            return new ProblemDetails().DefaultForbidden(defaultErrorTitle, _modelName);
        }

        return modelInDataBase;
    }

    private static bool UserHasPermission(T modelInDataBase, Guid contextUserId)
    {
        if (modelInDataBase.UserId != contextUserId)
        {
            return false;
        }

        return true;
    }

    private string BuildDefaultErrorTitle(Guid id, string action)
    {
        return $"Failed to {action} {_modelName} ID {id}.";
    }
}
