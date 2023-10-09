using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Serilog;
using Serilog.Context;
using Troublecat.Assets;
using Troublecat.Configuration;
using Troublecat.Core;
using Troublecat.Core.Assets.Fonts;
using Troublecat.Core.Graphics;
using Troublecat.Data.Serialization;
using Troublecat.IO;
using Troublecat.IO.Extensions;
using Troublecat.Utilities;

namespace Troublecat.Data;

public delegate void OnAfterAllContentLoadedEvent();

public class GameDataLoader : IDataLoader {
    protected readonly Dictionary<Type, HashSet<Guid>> _database = new();
    protected readonly Dictionary<Guid, Object> _allAssets = new();
    private ImmutableDictionary<string, PixelFont> _fonts = ImmutableDictionary<string, PixelFont>.Empty;
    protected readonly Dictionary<string, Effect> _effects = new();

    private readonly string _packedContentDirectory = "resources";
    private readonly ILogger<GameDataLoader> _logger;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly TroublecatConfiguration _config;
    private readonly IShaderProvider? _shaderProvider;
    private readonly AssetSerializer _serializer;
    public readonly CacheDictionary<string, Texture2D> CachedUniqueTextures = new(32);

    public ImmutableArray<string> AvailableUniqueTextures;

    public event OnAfterAllContentLoadedEvent OnAfterAllContentLoaded;

    public GameDataLoader(ILogger<GameDataLoader> logger, GraphicsDevice graphicsDevice, IOptions<TroublecatConfiguration> config, AssetSerializer serializer, IShaderProvider? shaderProvider = null) {
        _logger = logger;
        _graphicsDevice = graphicsDevice;
        _config = config.Value;
        _shaderProvider = shaderProvider;
        _serializer = serializer;
    }

    public Task LoadContentProgress { get; private set; } = Task.CompletedTask;

    public void LoadContent() {
        _database.Clear();
        LoadContentProgress = Task.Run(LoadContentAsyncInternal);
    }

    public T GetAsset<T>(string path) where T : class {
        path = ConvertPathToKey(path);
        if (typeof(T) == typeof(Effect) && _effects.TryGetValue(path, out var effect)) {
            return effect as T;
        }
        if (typeof(T) == typeof(PixelFont) && _fonts.TryGetValue(path, out var font)) {
            return font as T;
        }
        if (typeof(T) == typeof(Texture2D)) {
            return FetchTexture(path) as T;
        }
        if (typeof(T) == typeof(InternalTexture)) {
            return new InternalTexture(path) as T;
        }
        return default;
    }

    private Texture2D FetchTexture(string path) {
        if (CachedUniqueTextures.ContainsKey(path)) {
            return CachedUniqueTextures[path];
        }

        var texture = GetTextureFromFile(_graphicsDevice, Path.Join(_config.ResourcesPackDirectoryAbsolute, $"{path.EscapePath()}.png"), true);
        texture.Name = path;
        CachedUniqueTextures[path] = texture;

        return texture;
    }

    protected readonly string ShaderRelativePath = Path.Join("shaders", "{0}.mgfxo");

    protected async Task LoadContentAsyncInternal() {
        try {
            using (LogContext.PushProperty("logger", nameof(GameDataLoader))) {
                await Task.Yield();
                await LoadContentAsync();
                if (Paths.Exists(_config.ResourcesPackDirectory)) {
                    LoadAllAssetsAtPath(_config.ResourcesPackDirectory);
                    LoadFonts();
                } else {
                    _logger.LogError($"Packed directory has not been created. path: {_config.ResourcesPackDirectoryAbsolute}");
                }
            }
            _logger.LogInformation($"content load complete!");
            OnAfterAllContentLoaded?.Invoke();
        } catch (Exception e) {
            LoadContentProgress = Task.FromException(e);
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

    private void LoadFonts() {
        string? fontsFolder = Paths.GetPath(_config.ResourcesPackDirectory, "fonts");
        var uniqueTextures = ImmutableArray.CreateBuilder<string>();
        foreach (string file in Directory.EnumerateFiles(fontsFolder)) {
            if (Path.GetExtension(file) == ".png") {
                uniqueTextures.Add(Paths.GetPathWithoutExtension(Path.GetRelativePath(_config.ResourcesPackDirectoryAbsolute, file)));
            } else if (Path.GetExtension(file) == ".json") {
                LoadFont(file);
            }
        }
        AvailableUniqueTextures = uniqueTextures.ToImmutable();
    }

    private void LoadFont(string fontPath) {
        _logger.LogInformation($"Loading font '{fontPath}");
        var json = Files.ReadText(fontPath);
        var asset = _serializer.DeserializeGeneric<FontAsset>(json)!;
        // Game.Data.AddAsset(asset);

        PixelFont font = new(asset);
        var key = ConvertPathToKey(Paths.GetPathWithoutExtension(Path.GetRelativePath(_config.ResourcesPackDirectoryAbsolute, fontPath)));
        if (_fonts.ContainsKey(key)) {
            _logger.LogError($"Unable to load font: {fontPath}. Duplicate index found!");
            return;
        }

        // font.AddFontSize(XmlHelper.LoadXML(Path.Join(PackedBinDirectoryPath, "fonts", $"{fontName}.fnt")).DocumentElement!, AtlasId.None);
        _fonts = _fonts.Add(key, font);
        _logger.LogInformation($"font '{fontPath}' loaded");
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

    //
    // Summary:
    //     Creates a Microsoft.Xna.Framework.Graphics.Texture2D from a file, supported formats
    //     bmp, gif, jpg, png, tif and dds (only for simple textures). May work with other
    //     formats, but will not work with tga files. This internally calls Microsoft.Xna.Framework.Graphics.Texture2D.FromStream(Microsoft.Xna.Framework.Graphics.GraphicsDevice,System.IO.Stream).
    //
    // Parameters:
    //   graphicsDevice:
    //     The graphics device to use to create the texture.
    //
    //   path:
    //     The path to the image file.
    //
    // Returns:
    //     The Microsoft.Xna.Framework.Graphics.Texture2D created from the given file.
    //
    // Remarks:
    //     Note that different image decoders may generate slight differences between platforms,
    //     but perceptually the images should be identical.
    private static Texture2D GetTextureFromFile(GraphicsDevice graphicsDevice, string path, bool premultiplyAlpha) {
        if (path == null) {
            throw new ArgumentNullException("path");
        }
        var texture = Texture2D.FromFile(graphicsDevice, path);

        if (premultiplyAlpha) {
            var data = new Microsoft.Xna.Framework.Color[texture.Width * texture.Height];
            texture.GetData(data);

            for (int i = 0; i < data.Length; i++) {
                data[i] = data[i].MultiplyAlpha();
            }
        }
        return texture;
    }

    private string ConvertPathToKey(string path) {
        return path.Replace("\\", "/").Replace("//", "/");
    }
}
