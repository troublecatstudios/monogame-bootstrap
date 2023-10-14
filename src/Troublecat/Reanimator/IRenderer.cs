using Troublecat.Core.Graphics;

namespace Troublecat.Reanimator;

public interface IRenderer {
    bool IsVisible { get; }
    bool IsFlipped { get; }

    void SetSprite(InternalTexture sprite);

    void SetColor(Color color);
}
