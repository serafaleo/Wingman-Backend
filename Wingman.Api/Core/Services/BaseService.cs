using Wingman.Api.Core.Models;

namespace Wingman.Api.Core.Services;

public abstract class BaseService<T> where T : BaseModel
{
    public readonly string modelName = $"{typeof(T).Name}";
}
