using System.Numerics;
using Troublecat.Core.Graphics;

namespace Troublecat.Data.Importers.Aseprite;

public enum AsepriteImportStyle {
    TypicalGrid,
    FrameAsRow,
    Sliced,
}

public readonly struct AsepriteImportOptions {
    public bool EnableSliceImport { get; init; }
    public bool EnableMirrorX { get; init; }
    public bool EnableMirrorY { get; init; }
    public AsepriteImportStyle ImportStyle { get; init; }
    public Color TransparentColor { get; init; }
    public Vector2 SpritePivot { get; init; }
    public bool EnablePaletteMap { get; init; }
}
