using Wingman.Api.Core.Models;

namespace Wingman.Api.Core.Services;

public abstract class BaseService<T> where T : BaseModel
{
    protected readonly string _modelName = $"{typeof(T).Name}";
}
