using Microsoft.Xna.Framework.Graphics;
using Troublecat.Core.Graphics;

using XnaGameTime = Microsoft.Xna.Framework.GameTime;
using XnaVector2 = Microsoft.Xna.Framework.Vector2;
using XnaRectangle = Microsoft.Xna.Framework.Rectangle;
using XnaColor = Microsoft.Xna.Framework.Color;
using MathHelper = Microsoft.Xna.Framework.MathHelper;
using System.ComponentModel;
using Troublecat.Core;
using Troublecat.Data;
using Troublecat.Input;
using Microsoft.Xna.Framework.Input;

namespace GameBootstrap.Scenes;

public class PrimitiveScene : GameScene {
    private int _drawType = 0;
    private InputManager _inputManager;

    public override void LoadContent(IDataLoader dataLoader) {
        _inputManager = new InputManager(1);
    }

    public override void Update(Timing time) {
        base.Update(time);
        _inputManager.Update(time);

        if (_inputManager.IsButtonDown(Keys.OemComma)) {
            _drawType--;
            if (_drawType < 0) {
                _drawType = 3;
            }
        }

        if (_inputManager.IsButtonDown(Keys.OemPeriod)) {
            _drawType++;
            if (_drawType >= 3) {
                _drawType = 0;
            }
        }
    }
    public override void Draw(SpriteBatch spriteBatch) {
        base.Draw(spriteBatch);
        switch(_drawType) {
            case 0: {
                spriteBatch.DrawRectangle(new XnaRectangle(100, 100, 100, 200), XnaColor.Purple, 20);
                spriteBatch.DrawCircle(100, 100, 90, 3, Color.White * 0.2f);
                spriteBatch.DrawCircle(100, 100, 90, 4, Color.White * 0.3f);
                spriteBatch.DrawCircle(100, 100, 90, 5, Color.White * 0.4f);
                spriteBatch.DrawCircle(100, 100, 90, 6, Color.White * 0.5f);
                spriteBatch.DrawCircle(100, 100, 90, 7, Color.White * 0.6f);
                spriteBatch.DrawCircle(100, 100, 90, 8, Color.White * 0.7f);
                spriteBatch.DrawCircle(100, 100, 90, 9, Color.White * 0.8f);
                spriteBatch.DrawCircle(100, 100, 90, 10, Color.White * 0.9f);
                spriteBatch.DrawCircle(100, 100, 90, 20, Color.Red);
                spriteBatch.DrawCircle(300, 100, 100, 50, Color.Green, 10);
                spriteBatch.DrawCircle(new XnaVector2(600, 100), 40, 30, Color.Green, 30);
                break;
            }
            case 1: {
                spriteBatch.DrawArc(new XnaVector2(100, 150), 100, 180, MathHelper.ToRadians(180), MathHelper.ToRadians(180), XnaColor.Navy, 1);
                spriteBatch.DrawArc(new XnaVector2(100, 150), 100, 180, MathHelper.ToRadians(180), MathHelper.ToRadians(160), XnaColor.Navy * 0.9f, 2);
                spriteBatch.DrawArc(new XnaVector2(100, 150), 100, 180, MathHelper.ToRadians(180), MathHelper.ToRadians(140), XnaColor.Navy * 0.8f, 4);
                spriteBatch.DrawArc(new XnaVector2(100, 50), 100, 180, MathHelper.ToRadians(180), MathHelper.ToRadians(120), XnaColor.Navy * 0.7f, 6);
                spriteBatch.DrawArc(new XnaVector2(100, 150), 100, 180, MathHelper.ToRadians(180), MathHelper.ToRadians(90), XnaColor.Navy * 0.6f, 8);
                spriteBatch.DrawArc(new XnaVector2(100, 150), 100, 180, MathHelper.ToRadians(180), MathHelper.ToRadians(80), XnaColor.Navy * 0.5f, 10);
                spriteBatch.DrawArc(new XnaVector2(100, 150), 100, 180, MathHelper.ToRadians(180), MathHelper.ToRadians(65), XnaColor.Navy * 0.4f, 12);
                spriteBatch.DrawArc(new XnaVector2(100, 150), 100, 180, MathHelper.ToRadians(180), MathHelper.ToRadians(45), XnaColor.Navy * 0.3f, 14);
                spriteBatch.DrawArc(new XnaVector2(100, 150), 100, 180, MathHelper.ToRadians(180), MathHelper.ToRadians(12), XnaColor.Navy * 0.2f, 16);
                spriteBatch.DrawArc(new XnaVector2(100, 100), 80,  90,  MathHelper.ToRadians(90), MathHelper.ToRadians(220), XnaColor.Navy, 10);
                break;
            }
            case 2: {
                spriteBatch.DrawLine(new XnaVector2(20, 220), new XnaVector2(480, 280), Color.Orange);
                spriteBatch.DrawLine(new XnaVector2(120, 230), new XnaVector2(480, 290), Color.Orange, 2);
                spriteBatch.DrawLine(new XnaVector2(220, 240), new XnaVector2(480, 300), Color.Orange, 5);
                spriteBatch.DrawLine(new XnaVector2(120, 255), new XnaVector2(480, 315), Color.Orange, 10);
                spriteBatch.DrawLine(new XnaVector2(100, 100), 40.0f, MathHelper.ToRadians(270), Color.Green, 3);
                spriteBatch.DrawLine(new XnaVector2(103, 100), 40.0f, MathHelper.ToRadians(105), Color.Blue, 3);
                spriteBatch.DrawLine(new XnaVector2(101, 100), 40.0f, MathHelper.ToRadians(0), Color.Red, 3);
                break;
            }
            case 3: {
                spriteBatch.PutPixel(new XnaVector2(150, 150), Color.Red);
                spriteBatch.PutPixel(new XnaVector2(151, 150), Color.Red);
                spriteBatch.PutPixel(new XnaVector2(151, 151), Color.Red);
                spriteBatch.PutPixel(new XnaVector2(150, 151), Color.Red);
                spriteBatch.PutPixel(new XnaVector2(153, 150), Color.Red);
                spriteBatch.PutPixel(new XnaVector2(154, 150), Color.Red);
                spriteBatch.PutPixel(new XnaVector2(154, 151), Color.Red);
                spriteBatch.PutPixel(new XnaVector2(153, 151), Color.Red);
                break;
            }
        }
    }
}
