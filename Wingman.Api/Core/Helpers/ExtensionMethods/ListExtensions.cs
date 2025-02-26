namespace Wingman.Api.Core.Helpers.ExtensionMethods;

public static class ListExtensions
{
    public static bool IsNotNullOrEmpty<T>(this List<T> list)
    {
        return list is not null && list.Count > 0;
    }

    public static bool IsNullOrEmpty<T>(this List<T> list)
    {
        return !list.IsNotNullOrEmpty();
    }
}
