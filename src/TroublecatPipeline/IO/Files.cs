using System.Diagnostics;

namespace Troublecat.IO;

public static class Files {
    public static void SaveText(in string content, in string fullpath) {
        Debug.Assert(Path.IsPathRooted(fullpath));

        if (!Paths.Exists(fullpath))
        {
            string directoryName = Path.GetDirectoryName(fullpath)!;
            _ = Paths.GetOrCreateDirectory(directoryName);
        }

        File.WriteAllText(fullpath, content);
    }

    public static async Task SaveTextAsync(string content, string fullpath) {
        Debug.Assert(Path.IsPathRooted(fullpath));

        if (!Paths.Exists(fullpath))
        {
            string directoryName = Path.GetDirectoryName(fullpath)!;
            _ = Paths.GetOrCreateDirectory(directoryName);
        }

        await File.WriteAllTextAsync(fullpath, content);
    }
}
