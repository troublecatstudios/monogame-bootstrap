using System.ComponentModel;

namespace Troublecat.Core.Graphics;

public enum AtlasId {
    None,
    [Description("default")]
    Gameplay,
    [Description("editor")]
    Editor,
    [Description("static")]
    Static,
    [Description("temporary")]
    Temporary,
    [Description("preload")]
    Preload,
}
