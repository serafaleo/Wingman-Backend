using LanguageExt;
using Microsoft.AspNetCore.Mvc;
using Wingman.Api.Core.Models;

namespace Wingman.Api.Core.Services.Interfaces;

public interface ICommonService<T> where T : CommonModel
{
    public Task<Either<ProblemDetails, List<T>>> GetAllAsync(int page, int pageSize, Guid contextUserId);
    public Task<Either<ProblemDetails, T>> GetByIdAsync(Guid id, Guid contextUserId);
    public Task<Either<ProblemDetails, Unit>> CreateAsync(T model, Guid contextUserId);
    public Task<Either<ProblemDetails, Unit>> UpdateAsync(Guid id, T model, Guid contextUserId);
    public Task<Either<ProblemDetails, Unit>> DeleteByIdAsync(Guid id, Guid contextUserId);
}
