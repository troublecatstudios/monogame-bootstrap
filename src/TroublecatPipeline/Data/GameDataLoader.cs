using Microsoft.Xna.Framework;
using Serilog;
using Serilog.Context;
using Troublecat.IO;

namespace Troublecat.Data;

public class GameDataLoader {
    protected readonly Dictionary<Type, HashSet<Guid>> _database = new();
    protected readonly Dictionary<Guid, Object> _allAssets = new();

    private readonly string _packedContentDirectory = "resources";

    public GameDataLoader() {
    }

    public Task LoadContentProgress = Task.CompletedTask;

    public void LoadContent() {
        _database.Clear();
        LoadContentProgress = Task.Run(LoadContentAsyncInternal);
    }

    protected async Task LoadContentAsyncInternal() {
        using(LogContext.PushProperty("logger", nameof(GameDataLoader))) {
            await Task.Yield();
            await LoadContentAsync();
            if (Paths.Exists(_packedContentDirectory)) {
                LoadAllAssetsAtPath(_packedContentDirectory);
            } else {
                Log.Error($"Packed directory has not been created.");
            }
        }
    }

    protected virtual Task LoadContentAsync() => Task.CompletedTask;


    public void LoadAllAssetsAtPath(string directoryPath) {
        var absolutePath = Paths.GetPath(directoryPath);
        Log.Information($"Resources path: {absolutePath}");
        foreach(var file in Paths.GetAllFilesInFolder(absolutePath, "*.*", recursive: true)) {
            Log.Information($"Content file: {file}");
        }
    }
}
