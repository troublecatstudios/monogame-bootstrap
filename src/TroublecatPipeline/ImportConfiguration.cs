using Troublecat.IO;

namespace Troublecat;

public class FontImportOptions {
    public static readonly FontImportOptions Default = new();
    public int FontSize { get; set; } = 12;
}

public class ImportConfiguration {
    public string ResourcesBuildDirectory { get; set; } = string.Empty;
    public string ResourcesPackDirectory { get; set; } = string.Empty;

    public Dictionary<string, FontImportOptions> Fonts { get; set; } = new();

    public string ResourcesBuildDirectoryAbsolute => Paths.GetPath("../../../../../", ResourcesBuildDirectory);
    public string ResourcesPackDirectoryAbsolute => Paths.GetPath("../../../../../", ResourcesPackDirectory);

    public FontImportOptions GetOptionsForFontOrDefault(string fontName) {
        if (Fonts.TryGetValue(fontName, out var fontOptions)) {
            return fontOptions;
        }
        return FontImportOptions.Default;
    }
}
