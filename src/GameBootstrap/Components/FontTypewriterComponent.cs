using Microsoft.Xna.Framework.Graphics;
using Troublecat.Assets;
using Troublecat.Core;
using Troublecat.Math;

namespace GameBootstrap.Components;

public class FontTypewriterComponent : IUpdateableComponent, IDrawableComponent {
    private string _text = string.Empty;
    private float _elapsedTime = 0f;

    public FontAsset? Font { get; set; }
    public int CharactersPerSecond { get; set; }
    public int VisibleCharacters { get; set; }

    public void Reset() {
        _elapsedTime = 0f;
    }

    public void SetText(string text) {
        Reset();
        _text = text;
    }


    public void Update(Timing time) {
        _elapsedTime += time.Delta;
        VisibleCharacters = Maths.FloorToInt(CharactersPerSecond * _elapsedTime);
    }

    public void Draw(SpriteBatch spriteBatch) {
        if (Font == null) return;

    }
}
