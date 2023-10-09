using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using Troublecat.IO.Extensions;
using Troublecat.Math;
using Troublecat.Utilities;

namespace Troublecat.Core.Graphics;

/// <summary>
/// A texture atlas, the texture2D can be loaded and unloaded from the GPU at any time
/// We will keep the texture lists in memory all the time, though.
/// </summary>
public class AtlasTexture : IDisposable
{
    /// <summary>Used publically only for the json serializer</summary>
    public Dictionary<string, AtlasCoordinates> _entries = new(StringComparer.InvariantCultureIgnoreCase);

    [JsonIgnore]
    private GraphicsDevice? _graphicsDevice;

    public readonly string Name;
    public readonly AtlasId Id;
    private readonly ITextureFactory _textureFactory;
    private readonly ILogger<AtlasTexture> _logger;
    [JsonIgnore]
    private Texture2D[] _textures = null!;
    public Texture2D[] Textures
    {
        get
        {
            if (_textures is null)
            {
                _textures = _textureFactory.GetAtlasTextures(Id, Name);
            }

            return _textures!;
        }
    }

    internal AtlasTexture(string name, AtlasId id, ITextureFactory atlasTextureFactory, ILogger<AtlasTexture> logger)
    {
        Name = name;
        Id = id;
        _textureFactory = atlasTextureFactory;
        _logger = logger;
    }

    public bool Exist(string id) => _entries.ContainsKey(id.EscapePath());
    public int CountEntries => _entries.Count;
    public IEnumerable<AtlasCoordinates> GetAllEntries() => _entries.Values;

    public void PopulateAtlas(IEnumerable<(string id, AtlasCoordinates coord)> entries)
    {
        foreach (var entry in entries)
        {
            _entries[entry.id] = entry.coord;
        }
    }

    public bool HasId(string id)
    {
        return _entries.ContainsKey(id.EscapePath());
    }

    public bool TryGet(string id, out AtlasCoordinates coord)
    {
        if (_entries.TryGetValue(id.EscapePath(), out AtlasCoordinates result))
        {
            coord = result;
            return true;
        }
        coord = AtlasCoordinates.Empty;
        return false;
    }

    public AtlasCoordinates Get(string id)
    {
        if (_entries.TryGetValue(id.EscapePath(), out AtlasCoordinates coord))
        {
            return coord;
        }
        else
        {
            _logger.LogInformation($"Image '{id}' is missing from the atlas");
            throw new Exception($"Image '{id}' is missing from the atlas");
            // return Game.Data.FetchAtlas(AtlasId.Editor).Get("missingImage");
        }
    }

    public void UnloadTextures()
    {
        if (_textures != null)
            foreach (var t in _textures)
            {
                t.Dispose();
            }
    }

    public void Dispose()
    {
        UnloadTextures();
        _entries.Clear();
    }
}
