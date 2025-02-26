namespace Wingman.Api.Core.Helpers.ExtensionMethods;

public static class StringExtensions
{
    public static bool IsNullOrEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str);
    }

    public static bool IsNotNullOrEmpty(this string? str)
    {
        return !string.IsNullOrEmpty(str);
    }
}
