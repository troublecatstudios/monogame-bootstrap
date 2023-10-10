
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
using Troublecat.Input;

namespace GameBootstrap.Scenes;

public class TextScene : GameScene {
    private PixelFont _font;
    private PixelFont _debugFont;
    private Vector2[] _characterPositions;
    private float _shakeTime;
    private int _randomIndex;
    private float _effectIntensity = 0.085f;
    private float _effectPerScroll = 0.05f;

    private InputManager _inputManager;

    private static string _exampleText = "Welcome.";
    private int _lastScrollValue;
    private bool _lastEPressed;
    private bool _showStroke;
    private bool _showShadow;
    private bool _showDebugText;

    private string[] _fpsNames = new string[] { "8", "12", "24", "30", "60" };
    private float[] _fps = new float[] { Constants.FPS_8, Constants.FPS_12, Constants.FPS_24, Constants.FPS_30, Constants.FPS_60 };
    private int _fpsIndex;

    private int _effectIndex;
    private string[] _effectNames = new string[] { "Shake", "Bounce", "None" };

    public override void LoadContent(IDataLoader dataLoader) {
        _characterPositions = new Vector2[_exampleText.Length];
        _font = dataLoader.GetAsset<PixelFont>("fonts/ChevyRay-MagicBook");
        _debugFont = dataLoader.GetAsset<PixelFont>("fonts/CabinCondensed-Regular");
        _inputManager = new InputManager(1);
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

        _inputManager.Update(time);

        if (_inputManager.IsButtonDown(Keys.OemComma)) {
            _fpsIndex--;
            if (_fpsIndex < 0) {
                _fpsIndex = _fps.Length - 1;
            }
        }

        if (_inputManager.IsButtonDown(Keys.OemPeriod)) {
            _fpsIndex++;
            if (_fpsIndex >= _fps.Length) {
                _fpsIndex = 0;
            }
        }

        if (_inputManager.IsButtonDown(Keys.Space)) {
            _showDebugText = !_showDebugText;
        }

        if (_inputManager.IsButtonDown(Keys.P)) {
            _effectIndex++;
            if (_effectIndex > 2) {
                _effectIndex = 0;
            }
        }

        if (_shakeTime > _fps[_fpsIndex]) {
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
                if (_effectIndex == 0) {
                    _characterPositions[i] = DoShake(i, _randomIndex, 1f, _effectIntensity);
                } else if (_effectIndex == 1) {
                    _characterPositions[i] = DoBounce(i, time.Total, 1f);
                } else {
                    _characterPositions[i] = Vector2.Zero;
                }
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch) {
        DrawFont(_exampleText, _font, spriteBatch, CenterScreen, Vector2.One, Color.White, shadowOffset: new Vector2(5, 1), shadowColor: Color.Black, strokeColor: Color.Orange, positions: _characterPositions);

        var effectName = _effectNames[_effectIndex];
        DrawFont(effectName, _debugFont, spriteBatch, CenterScreen + Directions.Down * 32, Vector2.One, Color.White);

        var fpsText = $"{_fpsNames[_fpsIndex]} fps";
        DrawFont(fpsText, _debugFont, spriteBatch, CenterScreen + Directions.Down * 48, Vector2.One/2, Color.White);

        if (_showDebugText) {
            DrawDebugText(spriteBatch);
        }
    }

    private void DrawDebugText(SpriteBatch spriteBatch) {
        var baseTextPos = BaseScreenSize;
        baseTextPos += (Directions.Up * _debugFont.LineHeight * _exampleText.Length) + Directions.Up * 20;
        baseTextPos += Directions.Left * _debugFont.GetLineWidth("abcedfghijklmnopqrstuvwxyz0123456789");
        for(var i = 0; i < _characterPositions.Length; i++) {
            var pos = _characterPositions[i];
            var label = $"{_exampleText[i]}: {pos.X}, {pos.Y}";
            var scale = Vector2.One;
            DrawFont(label, _debugFont, spriteBatch, baseTextPos, scale, Color.White);
            baseTextPos += Directions.Down * _debugFont.LineHeight;
        }
    }

    private Vector2 DoBounce(int characterIndex, float timeTotal, float effectIntensity, float amplitude = 0.08f, float frequency = 1f, float waveSize = 0.08f) {
        float BounceTween(float t) {
            const float stillTime = .2f;
            const float easeIn = .2f;
            const float bounce = 1 - stillTime - easeIn;

            if (t <= easeIn)
                return Easings.EaseInOut(t / easeIn);
            t -= easeIn;

            if (t <= bounce)
                return 1 - Easings.BounceOut(t / bounce);

            return 0;
        }
        return Directions.Up * effectIntensity * BounceTween((Maths.Repeat(timeTotal * frequency - waveSize * characterIndex, 1))) * amplitude;
    }

    private static Vector2 DoShake(int characterIndex, int randomIndex, float shakeStrength, float effectIntensity) {
        var index = Maths.RoundToInt((characterIndex + randomIndex) % (Randoms.FakeRandomsCount - 1));
        var v3 = Randoms.FakeRandoms[index] * shakeStrength * effectIntensity;

        var x = Randoms.CoinFlip() * (float.IsNaN(v3.X) ? 0 : v3.X);
        var y = Randoms.CoinFlip() * (float.IsNaN(v3.Y) ? 0 : v3.Y);

        return new Vector2(x, y);
    }
}
