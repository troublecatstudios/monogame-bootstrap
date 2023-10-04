using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Serilog;
using Serilog.Context;
using Troublecat.Configuration;
using Troublecat.IO;

namespace Troublecat.Data;

public class GameDataLoader : IDataLoader {
    protected readonly Dictionary<Type, HashSet<Guid>> _database = new();
    protected readonly Dictionary<Guid, Object> _allAssets = new();

    protected readonly Dictionary<string, Effect> _effects = new();

    private readonly string _packedContentDirectory = "resources";
    private readonly ILogger<GameDataLoader> _logger;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly TroublecatConfiguration _config;
    private readonly IShaderProvider? _shaderProvider;

    public GameDataLoader(ILogger<GameDataLoader> logger, GraphicsDevice graphicsDevice, IOptions<TroublecatConfiguration> config, IShaderProvider? shaderProvider = null) {
        _logger = logger;
        _graphicsDevice = graphicsDevice;
        _config = config.Value;
        _shaderProvider = shaderProvider;
    }

    public Task LoadContentProgress = Task.CompletedTask;

    public void LoadContent() {
        _database.Clear();
        LoadContentProgress = Task.Run(LoadContentAsyncInternal);
    }

    public T GetAsset<T>(string path) where T : class {
        _logger.LogInformation($"Request for asset: {path}");
        if (typeof(T) == typeof(Effect) && _effects.TryGetValue(path, out var effect)) {
            return effect as T;
        }
        return default;
    }

    protected readonly string ShaderRelativePath = Path.Join("shaders", "{0}.mgfxo");

    protected async Task LoadContentAsyncInternal() {
        using (LogContext.PushProperty("logger", nameof(GameDataLoader))) {
            await Task.Yield();
            await LoadContentAsync();
            if (Paths.Exists(_config.ResourcesPackDirectory)) {
                LoadAllAssetsAtPath(_config.ResourcesPackDirectory);
            } else {
                _logger.LogError($"Packed directory has not been created. path: {_config.ResourcesPackDirectoryAbsolute}");
            }
        }
    }

    protected virtual Task LoadContentAsync() => Task.CompletedTask;


    public void LoadAllAssetsAtPath(string directoryPath) {
        var absolutePath = Paths.GetPath(directoryPath);
        _logger.LogInformation($"Resources path: {absolutePath}");
        foreach (var file in Paths.GetAllFilesInFolder(absolutePath, "*.*", recursive: true)) {
            _logger.LogInformation($"Content file: {file}");
        }
    }

    /// <summary>
    /// Override this to load all shaders present in the game.
    /// </summary>
    /// <param name="breakOnFail">Whether we should break if this fails.</param>
    /// <param name="forceReload">Whether we should force the reload (or recompile) of shaders.</param>
    private void LoadShaders(bool breakOnFail, bool forceReload = false) {
        // using PerfTimeRecorder recorder = new("Loading Shaders");
        var internalShaders = new string[] { "sprite2d", "simple", "bloom", "posterize", "mask" };
        foreach (var shader in internalShaders) {
            if (LoadShader(shader, out var result, breakOnFail, forceReload)) {
                _effects.Add(shader, result);
            }
        }

        if (_shaderProvider is IShaderProvider provider && provider.Shaders.Length > 0) {
            for (int i = 0; i < provider.Shaders.Length; i++) {
                if (LoadShader(provider.Shaders[i], out var shader, breakOnFail, forceReload)) {
                    _effects.Add(provider.Shaders[i], shader);
                }
            }
        }
    }

    /// <summary>
    /// Load and return shader of name <paramref name="name"/>.
    /// </summary>
    public bool LoadShader(string name, [NotNullWhen(true)] out Effect? effect, bool breakOnFail, bool forceReload) {
        _logger.Verify(_config.ResourcesPackDirectory is not null, "Why hasn't LoadContent() been called?");

        Effect? shaderFromFile = null;
        if (forceReload || !TryLoadShaderFromFile(name, out shaderFromFile)) {
            if (TryCompileShader(name, out Effect? compiledShader)) {
                effect = compiledShader;
                effect.Name = name;
                return true;
            }
        }

        if (shaderFromFile is not null) {
            effect = shaderFromFile;
            effect.Name = name;
            return true;
        }

        if (breakOnFail) {
            throw new InvalidOperationException("Unable to compile shader!");
        }

        effect = null;
        return false;
    }

    protected virtual bool TryCompileShader(string name, [NotNullWhen(true)] out Effect? result) {
        result = null;
        return false;
    }

    private string OutputPathForShaderOfName(string name, string? path = default) {
        _logger.Verify(_config.ResourcesPackDirectory is not null, "Why hasn't LoadContent() been called?");
        return Path.Join(path ?? _config.ResourcesPackDirectory, string.Format(ShaderRelativePath, name));
    }

    private bool TryLoadShaderFromFile(string name, [NotNullWhen(true)] out Effect? result) {
        string shaderPath = OutputPathForShaderOfName(name);
        result = null;
        try {
            result = new Effect(_graphicsDevice, File.ReadAllBytes(shaderPath));
        } catch {
            _logger.LogError($"Error loading file: {shaderPath}");
            return false;
        }

        return true;
    }
}
