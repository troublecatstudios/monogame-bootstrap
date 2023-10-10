using System.Numerics;
using Microsoft.Xna.Framework.Input;
using Troublecat.Core;

namespace Troublecat.Input;

public class InputManager {
    private KeyboardState _previousKeyboardState;
    private KeyboardState _currentKeyboardState;

    private MouseState _previousMouseState;
    private MouseState _currentMouseState;

    private GamePadState _previousGamepadState;
    private GamePadState _currentGamepadState;

    public InputManager(int playerIndex = 0) {
        PlayerIndex = playerIndex;
    }

    public int PlayerIndex { get; private set; } = 0;

    public bool ShouldUseGamepad { get; private set; } = false;

    public int GamepadPort { get; private set; } = 0;

    public bool IsGamepadConnected => GamepadCapabilities.IsConnected;

    public GamePadCapabilities GamepadCapabilities { get; private set; } = new();

    public Vector2 MousePosition => new Vector2(_currentMouseState.X, _currentMouseState.Y);
    public Vector2 MousePositionDelta => new Vector2(_previousMouseState.X, _previousMouseState.Y) - MousePosition;

    public Vector2 MouseScroll => new Vector2(_currentMouseState.HorizontalScrollWheelValue, _currentMouseState.ScrollWheelValue);
    public Vector2 MouseScrollDelta => new Vector2(_previousMouseState.HorizontalScrollWheelValue, _previousMouseState.ScrollWheelValue) - MouseScroll;

    public bool IsButtonDown(Keys key) => _currentKeyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
    public bool IsButtonHeld(Keys key) => _currentKeyboardState.IsKeyDown(key) && _previousKeyboardState.IsKeyDown(key);

    public void Update(Timing time) {
        _previousGamepadState = _currentGamepadState;
        _previousKeyboardState = _currentKeyboardState;
        _previousMouseState = _currentMouseState;

        _currentGamepadState = GamePad.GetState(PlayerIndex);
        _currentKeyboardState = Keyboard.GetState();
        _currentMouseState = Mouse.GetState();

        GamepadUpdate();
    }

    private void GamepadUpdate() {
        for (int i = 1; i <= 4; i++)
        {
            var state = GamePad.GetCapabilities(i);
            if (state.IsConnected && i == PlayerIndex)
            {
                GamepadPort = i;
                GamepadCapabilities = state;
            }
        }
    }
}
