
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Numerics;
using Troublecat;
using Troublecat.Core.Assets.Fonts;
using Troublecat.Core.Geometry;
using Troublecat.Data;
using Troublecat.Math;
using Troublecat.Utilities;

using XnaGameTime = Microsoft.Xna.Framework.GameTime;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaColor = Microsoft.Xna.Framework.Color;
using Troublecat.Core.Graphics;
using Troublecat.Core;

namespace GameBootstrap.Scenes;

public class TextScene : GameScene {
    private PixelFont _font;
    private PixelFont _debugFont;
    private Vector2[] _characterPositions;
    private float _shakeTime;
    private int _randomIndex;
    private float _effectIntensity = 0.085f;
    private float _effectPerScroll = 0.05f;
    private float _fps = Constants.FPS_24;

    private static string _exampleText = "Welcome.";
    private int _lastScrollValue;
    private bool _lastEPressed;
    private bool _showStroke;
    private bool _showShadow;



    public override void LoadContent(IDataLoader dataLoader) {
        _characterPositions = new Vector2[_exampleText.Length];
        _font = dataLoader.GetAsset<PixelFont>("fonts/ChevyRay-MagicBook");
        _debugFont = dataLoader.GetAsset<PixelFont>("fonts/Noir");
    }

    public override void Update(Timing time) {
        base.Update(time);
        var totalTime = time.Total;
        _shakeTime += time.Delta;

        var mouseState = Mouse.GetState();
        if (mouseState.ScrollWheelValue != 0) {
            var delta = mouseState.ScrollWheelValue - _lastScrollValue;
            var sign = MathF.Sign(delta);
            var scrollScaled = delta/120f;
            var scrollClamped = Maths.Clamp(scrollScaled, 0.1f, 8f) * sign;
            _effectIntensity += scrollClamped * _effectPerScroll;
            _effectIntensity = Maths.Clamp(_effectIntensity, 0.085f, 500f);
            _lastScrollValue = mouseState.ScrollWheelValue;
        }

        var kbState = Keyboard.GetState();
        if (kbState.IsKeyDown(Keys.E) && !_lastEPressed) {
            if (_fps == Constants.FPS_24) _fps = Constants.FPS_12;
            else _fps = Constants.FPS_24;
        }
        _lastEPressed = kbState.IsKeyDown(Keys.E);

        if (_shakeTime > 1f) {
            _shakeTime = 0f;
            var newRnd = Randoms.InRange(0, Randoms.FakeRandomsCount);
            //Avoids repeating the same index twice
            if (_randomIndex == newRnd) {
                newRnd++;
                if (newRnd >= Randoms.FakeRandomsCount)
                    newRnd = 0;
            }
            _randomIndex = newRnd;

            for(var i = 0; i < _exampleText.Length; i++) {
                _characterPositions[i] = DoShake(i, _randomIndex, 1f, _effectIntensity);
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch) {
        DrawFont(_exampleText, _font, spriteBatch, (BaseScreenSize/2), Vector2.One, Color.White, shadowOffset: new Vector2(5, 1), shadowColor: Color.Black, strokeColor: Color.Orange, positions: _characterPositions);

        var label = $"Testing";
        var width = _debugFont.GetLineWidth(label);
        var offset = Directions.Left * (width + 20) + Directions.Up * _debugFont.LineHeight;
        DrawFont(label, _debugFont, spriteBatch, BaseScreenSize + offset, Vector2.One, Color.White);
    }



    private Vector2 DoShake(int characterIndex, int randomIndex, float shakeStrength, float effectIntensity) {
        var index = Maths.RoundToInt((characterIndex + randomIndex) % (Randoms.FakeRandomsCount - 1));
        var v3 = Randoms.FakeRandoms[index] * shakeStrength * effectIntensity;
        return new Vector2(v3.X, v3.Y);
    }
}
