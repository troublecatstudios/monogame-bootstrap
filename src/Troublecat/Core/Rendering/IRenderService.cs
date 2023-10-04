using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Geometry;
using Troublecat.Core.Graphics;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace Troublecat.Core.Rendering;

public interface IRenderService {
    void DrawTextureQuad(Texture2D texture, Rectangle source, Rectangle destination, XnaMatrix matrix, Color color, Effect effect, BlendState blend, bool smoothing);
    void DrawTextureQuad(Texture2D texture, Rectangle source, Rectangle destination, XnaMatrix matrix, Color color, BlendState blend);
    void DrawTextureQuad(Texture2D texture, Rectangle source, Rectangle destination, XnaMatrix matrix, Color color, BlendState blend, Effect shaderEffect);
}
