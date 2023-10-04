using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Troublecat.Configuration;
using Troublecat.Core.Rendering;
using Troublecat.Utilities;

namespace Troublecat.Core.Graphics;

public class TextureFactory : ITextureFactory {
    private readonly ILogger<TextureFactory> _logger;
    private readonly TroublecatConfiguration _config;
    private readonly GraphicsDevice _graphicsDevice;
    private readonly IRenderService _renderService;
    private readonly IAtlasTextureFactory _atlasTextureFactory;

    public TextureFactory(ILogger<TextureFactory> logger, IOptions<TroublecatConfiguration> config, GraphicsDevice graphicsDevice, IRenderService renderService, IAtlasTextureFactory atlasTextureFactory) {
        _logger = logger;
        _config = config.Value;
        _graphicsDevice = graphicsDevice;
        _renderService = renderService;
        _atlasTextureFactory = atlasTextureFactory;
    }

    public AtlasTexture Create(AtlasId id, string name) {
        throw new NotImplementedException();
    }

    public Texture2D CreateTextureFromAtlas(AtlasCoordinates textureCoord, SurfaceFormat format = SurfaceFormat.Color, int scale = 1) {
        RenderTarget2D result =
            new RenderTarget2D(_graphicsDevice,
            System.Math.Max(1, textureCoord.SourceRectangle.Width) * scale,
            System.Math.Max(1, textureCoord.SourceRectangle.Height) * scale, false, format, DepthFormat.None);
        _graphicsDevice.SetRenderTarget(result);
        _graphicsDevice.Clear(Color.Transparent);
        // Draw the cropped image from the atlas
        var atlas = _atlasTextureFactory.GetAtlas(textureCoord.AtlasId);
        _renderService.DrawTextureQuad(
            atlas.Textures[textureCoord.AtlasIndex],
            textureCoord.SourceRectangle,
            result.Bounds,
            Microsoft.Xna.Framework.Matrix.Identity,
            Color.White,
            BlendState.AlphaBlend
            );

        // Return the graphics device to the screen
        _graphicsDevice.SetRenderTarget(null);
        result.Name = textureCoord.Name;

        return result;
    }

    public Texture2D[] GetAtlasTextures(AtlasId id, string name) {
        string atlasPath = Path.Join(_config.ResourcesPackDirectoryAbsolute, _config.AtlasFolderName);
        var atlasFiles = new DirectoryInfo(atlasPath).EnumerateFiles($"{id.GetDescription()}????.png").ToArray();

        if (atlasFiles.Length == 0)
        {
            throw new FileNotFoundException($"Atlas '{id}' not found in '{atlasPath}'");
        }

        var textures = new Texture2D[atlasFiles.Length];
        for (int i = 0; i < atlasFiles.Length; i++)
        {
            var path = atlasFiles[i].FullName;
            textures[i] = Texture2D.FromFile(_graphicsDevice, path);
            _logger.Verify(textures[i] is not null, $"Couldn't load atlas file at {path}");
        }
        return textures;
    }
}
