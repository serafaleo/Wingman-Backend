namespace Wingman.Api.Core.Helpers.ExtensionMethods;

public static class ExceptionExtensions
{
    public static string GetInnermostMessage(this Exception exception)
    {
        return exception.GetInnermostException().Message;
    }

    public static Exception GetInnermostException(this Exception exception)
    {
        Exception innermostException = exception;

        while (innermostException.InnerException is not null)
        {
            innermostException = innermostException.InnerException;
        }

        return innermostException;
    }
}
