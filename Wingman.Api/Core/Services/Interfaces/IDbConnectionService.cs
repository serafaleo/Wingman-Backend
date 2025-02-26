using System.Data;

namespace Wingman.Api.Core.Services.Interfaces;

public interface IDbConnectionService
{
    public IDbConnection CreateConnection();
}
