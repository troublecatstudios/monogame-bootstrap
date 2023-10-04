using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Geometry;
using Troublecat.Core.Graphics;
using Troublecat.Data;
using Troublecat.Utilities;
using XnaMatrix = Microsoft.Xna.Framework.Matrix;
using XnaVector3 = Microsoft.Xna.Framework.Vector3;

namespace Troublecat.Core.Rendering;

public class RenderService : IRenderService {
    internal static Microsoft.Xna.Framework.Vector3 BLEND_NORMAL = new(1, 0, 0);
    internal static Microsoft.Xna.Framework.Vector3 BLEND_WASH = new(0, 1, 0);
    internal static Microsoft.Xna.Framework.Vector3 BLEND_COLOR_ONLY = new(0, 0, 1);
    private readonly GraphicsDevice _graphicsDevice;
    private readonly IDataLoader _dataLoader;
    private readonly VertexInfo[] _cachedVertices = new VertexInfo[4];
    private readonly short[] _cachedIndices = new short[6];

    public RenderService(GraphicsDevice graphicsDevice, IDataLoader dataLoader) {
        _graphicsDevice = graphicsDevice;
        _dataLoader = dataLoader;
        for (int i = 0; i < 4; i++) {
            _cachedVertices[i] = new VertexInfo();
        }
    }

    public void DrawTextureQuad(Texture2D texture, Rectangle source, Rectangle destination, XnaMatrix matrix, Color color, Effect effect, BlendState blend, bool smoothing) {
        (VertexInfo[] verts, short[] indices) = MakeTexturedQuad(destination, source, new Vector2(texture.Width, texture.Height), color, BLEND_NORMAL);
        _graphicsDevice.SamplerStates[0] = smoothing ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
        DrawIndexedVertices(matrix, verts, verts.Length, indices, indices.Length / 3, effect, blend, texture);
    }

    public void DrawTextureQuad(Texture2D texture, Rectangle source, Rectangle destination, XnaMatrix matrix, Color color, BlendState blend)
    {
        (VertexInfo[] verts, short[] indices) = MakeTexturedQuad(destination, source, new Vector2(texture.Width, texture.Height), color, BLEND_NORMAL);

        // if (blend == BlendState.Additive)
        //     Game.Data.ShaderSprite.SetTechnique("Add");
        // else
        //     Game.Data.ShaderSprite.SetTechnique("Alpha");

        var shaderSprite = _dataLoader.GetAsset<Effect>("sprite2d");
        DrawIndexedVertices(
            matrix,
            verts, verts.Length, indices, indices.Length / 3, shaderSprite,
            blend,
            texture);
    }

    public void DrawTextureQuad(Texture2D texture, Rectangle source, Rectangle destination, XnaMatrix matrix, Color color, BlendState blend, Effect shaderEffect)
    {
        (VertexInfo[] verts, short[] indices) = MakeTexturedQuad(destination, source, new Vector2(texture.Width, texture.Height), color, BLEND_NORMAL);

        DrawIndexedVertices(
            matrix,
            verts, verts.Length, indices, indices.Length / 3, shaderEffect,
            blend,
            texture);
    }

    private (VertexInfo[] vertices, short[] indices) MakeTexturedQuad(Rectangle destination, Rectangle source, Vector2 sourceSize, Color color, XnaVector3 BlendType) {
        // 0---1
        // |\  |
        // | \ |
        // |  \|
        // 3---2

        Vector2 uvTopLeft = new(source.X / sourceSize.X, source.Y / sourceSize.Y);
        Vector2 uvTopRight = new((source.X + source.Width) / sourceSize.X, source.Y / sourceSize.Y);
        Vector2 uvBottomRight = new((source.X + source.Width) / sourceSize.X, (source.Y + source.Height) / sourceSize.Y);
        Vector2 uvBottomLeft = new(source.X / sourceSize.X, (source.Y + source.Height) / sourceSize.Y);

        _cachedVertices[0].Position = destination.TopLeft.ToXnaVector3();
        _cachedVertices[0].Color = color;
        _cachedVertices[0].TextureCoordinate = uvTopLeft;
        _cachedVertices[0].BlendType = BlendType;

        _cachedVertices[1].Position = destination.TopRight.ToXnaVector3();
        _cachedVertices[1].Color = color;
        _cachedVertices[1].TextureCoordinate = uvTopRight;
        _cachedVertices[1].BlendType = BlendType;

        _cachedVertices[2].Position = destination.BottomRight.ToXnaVector3();
        _cachedVertices[2].Color = color;
        _cachedVertices[2].TextureCoordinate = uvBottomRight;
        _cachedVertices[2].BlendType = BlendType;

        _cachedVertices[3].Position = destination.BottomLeft.ToXnaVector3();
        _cachedVertices[3].Color = color;
        _cachedVertices[3].TextureCoordinate = uvBottomLeft;
        _cachedVertices[3].BlendType = BlendType;

        _cachedIndices[0] = 0;
        _cachedIndices[1] = 1;
        _cachedIndices[2] = 2;
        _cachedIndices[3] = 0;
        _cachedIndices[4] = 2;
        _cachedIndices[5] = 3;


        return (_cachedVertices, _cachedIndices);
    }

    private void DrawIndexedVertices<T>(XnaMatrix matrix, T[] vertices, int vertexCount, short[] indices, int primitiveCount, Effect effect, BlendState? blendState = null, Texture2D? texture = null) where T : struct, IVertexType {
        var b = blendState ?? BlendState.AlphaBlend;

        var size = new Vector2(_graphicsDevice.Viewport.Width, _graphicsDevice.Viewport.Height);

        matrix *= XnaMatrix.CreateScale((1f / size.X) * 2, -(1f / size.Y) * 2, 1f); // scale to relative points
        matrix *= XnaMatrix.CreateTranslation(-1f, 1f, 0); // translate to relative points

        _graphicsDevice.RasterizerState = RasterizerState.CullNone;
        _graphicsDevice.BlendState = b;
        _graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;

        effect.Parameters["MatrixTransform"].SetValue(matrix);

        if (texture != null) {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                _graphicsDevice.Textures[0] = texture;
                _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexCount, indices, 0, primitiveCount);
            }
        } else // Saving that 1 check for performance
          {
            foreach (EffectPass pass in effect.CurrentTechnique.Passes) {
                pass.Apply();
                _graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, vertices, 0, vertexCount, indices, 0, primitiveCount);
            }
        }
    }
}
