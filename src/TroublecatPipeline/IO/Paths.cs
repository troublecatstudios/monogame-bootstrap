namespace Troublecat.IO;

public static class Paths {
    private static readonly char[] BasicTrimCharacters = new[] { ' ', '-', '!', '@', ',', '#', '$', '%', '^', '&', '*' };
    public static string GetSafeFileName(string filePath) {
        return GetFileName(filePath, includeExtension: false, BasicTrimCharacters);
    }
    public static string GetFileName(string filePath, bool includeExtension = true, params char[] trimCharacters) {
        var name = includeExtension ? Path.GetFileName(filePath) : Path.GetFileNameWithoutExtension(filePath);
        if (trimCharacters.Any()) { // TODO: not sure if a regex is faster here
            for (var i = 0; i < trimCharacters.Length; i++) {
                var idx = name.IndexOf(trimCharacters[i]);
                if (idx != -1) {
                    name = name.Remove(idx, 1);
                }
            }
        }
        return name;
    }

    /// <summary>
    /// Gets the rooted path from a relative one
    /// </summary>
    /// <param name="paths"></param>
    /// <returns></returns>
    public static string GetPath(params string[] paths) {
        var path = Path.Join(paths);

        if (Path.IsPathRooted(path)) {
            // Already rooted, so yay?
            return path;
        }

        return Path.GetFullPath(Path.Join(Path.GetDirectoryName(AppContext.BaseDirectory), path));
    }

    public static IEnumerable<string> ListAllDirectories(string path) {
        if (!Directory.Exists(path)) {
            return Enumerable.Empty<string>();
        }

        return Directory.GetDirectories(path);
    }

    public static IEnumerable<FileInfo> GetAllFilesInFolder(string path, string filter, bool recursive) {
        if (!Directory.Exists(path)) {
            return Enumerable.Empty<FileInfo>();
        }

        DirectoryInfo dir = new(path);
        return dir.EnumerateFiles(filter, recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
    }

    public static IEnumerable<FileInfo> GetAllFilesInFolder(string path, bool recursive, params string[] filters) {
        List<FileInfo> result = new();
        foreach (string filter in filters) {
            result.AddRange(GetAllFilesInFolder(path, filter, recursive));
        }

        return result;
    }

    public static DirectoryInfo GetOrCreateDirectory(in string path) {
        if (Directory.Exists(path)) {
            return new DirectoryInfo(path);
        }

        return Directory.CreateDirectory(path);
    }

    public static bool DeleteDirectoryIfExists(in string path) {
        if (Directory.Exists(path)) {
            Directory.Delete(path, true);
            return true;
        }

        return false;
    }

    public static bool Exists(in string path) {
        var absolutePath = GetPath(path);
        if (Path.HasExtension(absolutePath)) {
            if (!Directory.Exists(Path.GetDirectoryName(absolutePath))) {
                return false;
            }
            if (!File.Exists(absolutePath)) {
                return false;
            }
            return true;
        }
        return Directory.Exists(absolutePath);
    }
}
