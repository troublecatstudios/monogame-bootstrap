namespace Troublecat.IO.Extensions;

/// <summary>
/// Allow other classes to pull in certain path methods as extensions without polluting other namespaces ;)
/// </summary>
public static class StringExtensions {
    public static string EscapePath(this string path) {
        return Paths.EscapePath(path);
    }
}
