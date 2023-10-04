using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XnaColor = Microsoft.Xna.Framework.Color;
namespace Troublecat.Core.Graphics;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexInfo : IVertexType {
    public Vector3 Position;
    public XnaColor Color;
    public Vector2 TextureCoordinate;
    public Vector3 BlendType;

    public static readonly VertexDeclaration VertexDeclaration;


    VertexDeclaration IVertexType.VertexDeclaration
    {
        get
        {
            return VertexDeclaration;
        }
    }

    static VertexInfo()
    {
        var elements = new VertexElement[]
        {
            new VertexElement(
                0,
                VertexElementFormat.Vector3,
                VertexElementUsage.Position, 0
            ),
            new VertexElement(
                12, // the size of the previous elements
                VertexElementFormat.Color,
                VertexElementUsage.Color, 0
            ),
            new VertexElement(
                16,
                VertexElementFormat.Vector2,
                VertexElementUsage.TextureCoordinate, 0
            ),
            new VertexElement(
                24,
                VertexElementFormat.Vector3,
                VertexElementUsage.TextureCoordinate, 1
            )
        };

        VertexDeclaration = new VertexDeclaration(elements);
    }

    public VertexInfo(Vector3 position, XnaColor color, Vector2 textureCoord, Vector3 blend)
    {
        Position = position;
        Color = color;
        BlendType = blend;
        TextureCoordinate = textureCoord;
    }

    public override int GetHashCode()
    {
        // TODO: Fix GetHashCode
        return (Position.GetHashCode() + Color.GetHashCode() + BlendType.GetHashCode())/3;
    }

    public override string ToString()
    {
        return (
            "{{Position:" + Position.ToString() +
            " Color:" + Color.ToString() +
            " Blend:" + BlendType.ToString() +
            "}}"
        );
    }

    public static bool operator ==(VertexInfo left, VertexInfo right)
    {
        return ((left.Color == right.Color) &&
                (left.Position == right.Position) &&
                (left.BlendType == right.BlendType));
    }

    public static bool operator !=(VertexInfo left, VertexInfo right)
    {
        return !(left == right);
    }

    public override bool Equals(object? obj)
    {
        if (obj == null)
        {
            return false;
        }
        if (obj.GetType() != base.GetType())
        {
            return false;
        }
        return (this == ((VertexInfo)obj));
    }
}
