using System.Diagnostics;
using System.Text;

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

    public static string ReadText(string filePath, Encoding? encoding = null) {
        encoding ??= Encoding.UTF8;
        if (!Paths.Exists(filePath)) {
            throw new FileNotFoundException($"Unable to find the file '{filePath}'!");
        }
        return File.ReadAllText(filePath, encoding);
    }
}
