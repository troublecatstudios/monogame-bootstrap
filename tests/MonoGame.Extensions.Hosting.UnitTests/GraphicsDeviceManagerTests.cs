using Microsoft.Xna.Framework;
using System.Reflection;

namespace MonoGame.Extensions.Hosting.UnitTests;

public class GraphicsDeviceManagerTests
{
    [Fact]
    public void Ensure_that_Game_class_has_intance_property()
    {
        var graphicsDeviceManagerProperty = typeof(Game)
            .GetProperty("graphicsDeviceManager", BindingFlags.Instance | BindingFlags.NonPublic);

        Assert.NotNull(graphicsDeviceManagerProperty);
    }
}
