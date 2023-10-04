using Microsoft.Extensions.Logging;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Troublecat;
using Troublecat.Assets;
using Troublecat.Data;

namespace GameBootstrap;

internal class BootstrappedGame : TroublecatGame {
    private FontAsset _testFont;

    public BootstrappedGame(ILogger<BootstrappedGame> logger, IServiceProvider serviceProvider) : base(logger, serviceProvider)
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    protected override void LoadContent()
    {
        base.LoadContent();
        // do any custom content loading here...
        //_testFont = Loader.GetAsset<FontAsset>("fonts/ChevyRay-MagicBook");
    }

    protected override void Update(GameTime gameTime)
    {
        if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);
    }
}
