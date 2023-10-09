using System.Numerics;
using Troublecat.Core.Geometry;
using Troublecat.Math;
using SystemVector4 = System.Numerics.Vector4;
using XnaColor = Microsoft.Xna.Framework.Color;
using XnaVector4 = Microsoft.Xna.Framework.Vector4;
using Effect = Microsoft.Xna.Framework.Graphics.Effect;
using Texture2D = Microsoft.Xna.Framework.Graphics.Texture2D;

namespace Troublecat.Utilities;

public static class XnaExtensions
{
    public static SystemVector4 ToSysVector4(this XnaColor color)
    {
        // Use XNA converter.
        XnaVector4 vec4 = color.ToVector4();
        return new SystemVector4(vec4.X, vec4.Y, vec4.Z, vec4.W);
    }

    public static Vector2 ToSysVector2(this Microsoft.Xna.Framework.Point point) => new((float)point.X, (float)point.Y);
    public static Vector2 ToSysVector2(this Microsoft.Xna.Framework.Vector2 vector) => new(vector.X, vector.Y);

    public static XnaColor ToXnaColor(this SystemVector4 color)
    {
        return new XnaColor(color.X, color.Y, color.Z, color.W);
    }

    /// <summary>
    /// Parses a string <paramref name="hex"/> to <see cref="Vector4"/>.
    /// </summary>
    /// <param name="hex">The string as the hex value, e.g. "#ff5545".</param>
    /// <returns>The converted color.</returns>
    public static Vector4 ToVector4Color(this string hex) {
        var rgba = System.Drawing.ColorTranslator.FromHtml(hex);
        return new Vector4(rgba.R/256f, rgba.G/256f, rgba.B/256f, 1);
    }

    public static Microsoft.Xna.Framework.Color MultiplyAlpha(this Microsoft.Xna.Framework.Color color)
    {
        return new Microsoft.Xna.Framework.Color(color.R * color.A, color.G * color.A, color.B * color.A, color.A);
    }

    public static Point ToPoint(this Vector2 vector2) =>
        new(Maths.RoundToInt(vector2.X), Maths.RoundToInt(vector2.Y));

    public static Rectangle ToRectangle(float x, float y, float width, float height)
    {
        int ix = Maths.RoundToInt(x);
        int iy = Maths.RoundToInt(y);
        int iwidth = Maths.RoundToInt(width);
        int iheight = Maths.RoundToInt(height);

        return new Rectangle(ix, iy, iwidth, iheight);
    }


    // Shader Extensions
    // ------------------------


    public static void SetTechnique(this Effect effect, string id)
    {
        if (effect.CurrentTechnique != effect.Techniques[id])
        {
            effect.CurrentTechnique = effect.Techniques[id];
        }

        // Funny enough, this is consuming way too much memory.
        // GameLogger.Verify(effect.CurrentTechnique != null, $"Skipping technique {id} for shader effect.");
    }

    public static void TrySetParameter(this Effect effect, string id, bool val)
    {
        if (effect.Parameters[id] != null)
        {
            effect.Parameters[id].SetValue(val);
        }
    }

    public static void SetParameter(this Effect effect, string id, bool val)
    {
        if (effect.Parameters[id] != null)
        {
            effect.Parameters[id].SetValue(val);
        }
        else
        {
            // TODO: re-enable this logger line
            // GameLogger.Warning($"Shader param '{id}' wasn't found");
        }
    }

    public static void TrySetParameter(this Effect effect, string id, int val)
    {
        if (effect.Parameters[id] != null)
        {
            effect.Parameters[id].SetValue(val);
        }
    }

    public static void SetParameter(this Effect effect, string id, int val)
    {
        if (effect.Parameters[id] != null)
        {
            effect.Parameters[id].SetValue(val);
        }
        else
        {
            // TODO: re-enable this logger line
            // GameLogger.Warning($"Shader param '{id}' wasn't found");
        }
    }

    public static void TrySetParameter(this Effect effect, string id, float val)
    {
        if (effect.Parameters[id] != null)
        {
            effect.Parameters[id].SetValue(val);
        }
    }
    public static void SetParameter(this Effect effect, string id, float val)
    {
        if (effect.Parameters[id] != null)
        {
            effect.Parameters[id].SetValue(val);
        }
        else
        {
            // TODO: re-enable this logger line
            // GameLogger.Warning($"Shader param '{id}' wasn't found");
        }
    }

    public static void SetParameter(this Effect effect, string id, Microsoft.Xna.Framework.Vector3[] val)
    {
        if (effect.Parameters[id] != null)
        {
            effect.Parameters[id].SetValue(val);
        }
        else
        {
            // TODO: re-enable this logger line
            // GameLogger.Warning($"Shader param '{id}' wasn't found");
        }
    }

    public static void SetParameter(this Effect effect, string id, Point val) =>
        SetParameter(effect, id, new Microsoft.Xna.Framework.Vector2(val.X, val.Y));
    public static void SetParameter(this Effect effect, string id, Vector2 val) =>
        SetParameter(effect, id, new Microsoft.Xna.Framework.Vector2(val.X, val.Y));

    public static void SetParameter(this Effect effect, string id, Microsoft.Xna.Framework.Vector2 val)
    {
        if (effect.Parameters[id] != null)
        {
            effect.Parameters[id].SetValue(val);
        }
        else
        {
            // TODO: re-enable this logger line
            // GameLogger.Warning($"Shader param '{id}' wasn't found");
        }
    }

    public static void SetParameter(this Effect effect, string id, Texture2D val)
    {
        if (effect.Parameters[id] != null)
        {
            effect.Parameters[id].SetValue(val);
        }
        else
        {
            // TODO: re-enable this logger line
            // GameLogger.Warning($"Shader param '{id}' wasn't found");
        }
    }
}
