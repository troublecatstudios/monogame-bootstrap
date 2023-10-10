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
}
