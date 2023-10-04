using System.Collections.Immutable;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SkiaSharp;
using Troublecat.Assets;
using Troublecat.Core.Assets.Fonts;
using Troublecat.Core.Diagnostics;
using Troublecat.Core.Geometry;
using Troublecat.Data.Serialization;
using Troublecat.IO;
using Troublecat.Math;

namespace Troublecat.Data.Importers;

[ResourceImporter("*.ttf")]
internal class FontImporter : IResourceImporter {
    private readonly ILogger<FontImporter> _logger;
    private readonly ImportConfiguration _configuration;
    private readonly AssetSerializer _serializer;

    public FontImporter(ILogger<FontImporter> logger, IOptions<ImportConfiguration> configuration, AssetSerializer serializer) {
        _logger = logger;
        _configuration = configuration.Value;
        _serializer = serializer;
    }

    public async Task ImportAsync(string resourcePath) {
        using(_logger.BeginScope(new PerfScope())) {
            _logger.LogInformation($"Importing file {resourcePath}");
            if (!Paths.Exists(resourcePath)) {
                _logger.LogError($"Resource not found! Does '{resourcePath}' exist?!");
                return;
            }
            var packPath = Paths.GetPath(_configuration.ResourcesPackDirectoryAbsolute, "Fonts");
            var fileName = Path.GetFileNameWithoutExtension(resourcePath);
            var jsonFile = $"{fileName}.json";
            var pngFile = $"{fileName}.png";
            var fontOptions = _configuration.GetOptionsForFontOrDefault(fileName);

            var jsonPackPath = Paths.GetPath(packPath, jsonFile);
            var pngPackPath = Paths.GetPath(packPath, pngFile);

            _ = Paths.GetOrCreateDirectory(packPath);
            using SKFileWStream stream = new(pngPackPath);
            using SKPaint skPaint = new ();

            skPaint.IsAntialias = false;
            skPaint.TextSize = fontOptions.FontSize;
            skPaint.Typeface = SKTypeface.FromFile(resourcePath);
            skPaint.Color = new SKColor(255, 255, 255);
            skPaint.TextAlign = SKTextAlign.Left;

            var bounds = new SKRect();
            var characters = new Dictionary<int, PixelFontCharacter>();
            var maxWidth = 0;
            var nextPosition = 0;

            SKFontMetrics fontMetrics = new SKFontMetrics();
            skPaint.GetFontMetrics(out fontMetrics);

            _logger.LogInformation($"Measuring characters...");

            // Measure each character and store in dictionary
            for (int i = 32; i < 127; i++)
            {
                string charAsString = ((char)i).ToString();
                var adjust = skPaint.MeasureText(charAsString, ref bounds);
                var advance = skPaint.GetGlyphWidths(charAsString).FirstOrDefault();
                var offset = skPaint.GetGlyphPositions(charAsString).FirstOrDefault();
                Point size = new((int)bounds.Width, (int)bounds.Height);

                var character = new PixelFontCharacter(i, new(nextPosition, 0, size.X, size.Y), (int)bounds.Left, (int)bounds.Top, Maths.CeilToInt(advance));
                characters[i] = character;
                maxWidth = System.Math.Max(maxWidth, (int)bounds.Width);
                nextPosition += size.X + 1;
            }

            _logger.LogInformation($"Calculating kernings...");

            // Calculate kerning for each pair of characters
            var kernings = new List<Kerning>();
            for (int i = 32; i < 127; i++)
            {
                for (int j = 32; j < 127; j++)
                {
                    ushort[] pair = new ushort[] { (ushort)i, (ushort)j };
                    var adjustments = skPaint.Typeface.GetKerningPairAdjustments(pair);
                    if (adjustments.Length > 0)
                    {
                        kernings.Add(new Kerning { First = i, Second = j, Amount = adjustments[0] });
                    }
                }
            }

            _logger.LogInformation($"Reticulating splines...");

            {
                using SKBitmap bitmap = new(nextPosition, nextPosition, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
                using SKCanvas canvas = new(bitmap);

                // Draw each character onto the bitmap
                foreach (var character in characters)
                {
                    var glyph = character.Value.Glyph;
                    canvas.DrawText(((char)character.Key).ToString(),
                        glyph.Left - character.Value.XOffset, glyph.Top - character.Value.YOffset,
                        skPaint);
                }

                // Save bitmap to PNG
                bitmap.Encode(stream, SKEncodedImageFormat.Png, 100);
            }

            FontAsset fontAsset = new(characters, kernings.ToImmutableArray(), fontOptions.FontSize, resourcePath, -fontMetrics.Ascent);
            string json = await _serializer.SerializeAsync(fontAsset, false);
            await Files.SaveTextAsync(json, jsonPackPath);

            _logger.LogInformation($"Font import complete png={pngPackPath}, json={jsonPackPath}");
        }
    }
}
