using GBJamGame.Enums;
using GBJamGame.Globals;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GBJamGame
{
    public class Input
    {
        private readonly IEnumerable<Actions> _actions;
        private readonly Dictionary<Actions, List<Buttons>> _buttonMappings;

        private readonly Dictionary<Actions, bool> _isPressed;
        private readonly Dictionary<Actions, List<Keys>> _keyMappings;
        private readonly Dictionary<Actions, float> _pressedTime;
        private readonly Dictionary<Actions, bool> _wasPressed;
        private bool _isTransitioning;

        public Input()
        {
            _keyMappings = new Dictionary<Actions, List<Keys>>
            {
                {Actions.A, new List<Keys> {Keys.Z, Keys.P}},
                {Actions.B, new List<Keys> {Keys.X, Keys.O}},
                {Actions.DPadUp, new List<Keys> {Keys.W, Keys.Up}},
                {Actions.DPadDown, new List<Keys> {Keys.S, Keys.Down}},
                {Actions.DPadLeft, new List<Keys> {Keys.A, Keys.Left}},
                {Actions.DPadRight, new List<Keys> {Keys.D, Keys.Right}},
                {Actions.Start, new List<Keys> {Keys.Enter}},
                {Actions.Select, new List<Keys> {Keys.Back}},
                {Actions.Fullscreen, new List<Keys> {Keys.F1}},
                {Actions.Screenshot, new List<Keys> {Keys.F2}},
                {Actions.ScreenshotGbRes, new List<Keys> {Keys.F3}}
            };

            _buttonMappings = new Dictionary<Actions, List<Buttons>>
            {
                {Actions.A, new List<Buttons> {Buttons.A, Buttons.Y}},
                {Actions.B, new List<Buttons> {Buttons.B, Buttons.X}},
                {Actions.DPadUp, new List<Buttons> {Buttons.DPadUp, Buttons.LeftThumbstickUp}},
                {Actions.DPadDown, new List<Buttons> {Buttons.DPadDown, Buttons.LeftThumbstickDown}},
                {Actions.DPadLeft, new List<Buttons> {Buttons.DPadLeft, Buttons.LeftThumbstickLeft}},
                {Actions.DPadRight, new List<Buttons> {Buttons.DPadRight, Buttons.LeftThumbstickRight}},
                {Actions.Start, new List<Buttons> {Buttons.Start}},
                {Actions.Select, new List<Buttons> {Buttons.Back}},
                {Actions.Screenshot, new List<Buttons> {Buttons.RightStick}}
            };

            _isPressed = new Dictionary<Actions, bool>();
            _pressedTime = new Dictionary<Actions, float>();
            _wasPressed = new Dictionary<Actions, bool>();

            _actions = Enum.GetValues(typeof(Actions)).Cast<Actions>();
            foreach (var input in _actions)
            {
                if (!_keyMappings.ContainsKey(input))
                    _keyMappings.Add(input, new List<Keys>());

                if (!_buttonMappings.ContainsKey(input))
                    _buttonMappings.Add(input, new List<Buttons>());

                _isPressed.Add(input, false);
                _pressedTime.Add(input, 0);
                _wasPressed.Add(input, false);
            }
        }

        private static bool IsButtonDown(GamePadState state, IEnumerable<Buttons> buttons)
        {
            return state.IsConnected
                   && buttons.Any(state.IsButtonDown);
        }

        private static bool IsKeyDown(KeyboardState state, IEnumerable<Keys> keys)
        {
            return keys.Any(state.IsKeyDown);
        }

        public bool Pressed(Actions action)
        {
            if (_isTransitioning)
                return false;

            if (!_isPressed.ContainsKey(action) || !_wasPressed.ContainsKey(action))
                return false;

            return _isPressed[action] && !_wasPressed[action];
        }

        public bool Released(Actions action)
        {
            if (_isTransitioning)
                return false;

            if (!_isPressed.ContainsKey(action) || !_wasPressed.ContainsKey(action))
                return false;

            return !_isPressed[action] && _wasPressed[action];
        }

        public bool Down(Actions action)
        {
            if (_isTransitioning)
                return false;

            return _isPressed.ContainsKey(action) && _isPressed[action];
        }

        public bool Repeat(Actions action, float initial)
        {
            if (_isTransitioning)
                return false;

            return !(PressedTime(action) < initial) && Down(action);
        }

        public bool Up(Actions action)
        {
            if (_isTransitioning)
                return false;

            return !Down(action);
        }

        public float PressedTime(Actions action)
        {
            if (_isTransitioning)
                return 0;

            return _pressedTime.ContainsKey(action)
                ? _pressedTime[action]
                : 0;
        }

        public void Update(GameTime gameTime, bool isTransitioning)
        {
            _isTransitioning = isTransitioning;
            var ksState = Keyboard.GetState();
            var gpState = GamePad.GetState(PlayerIndex.One);

            foreach (var action in _actions)
            {
                _wasPressed[action] = _isPressed[action];

                if (IsKeyDown(ksState, _keyMappings[action]) ||
                    IsButtonDown(gpState, _buttonMappings[action]))
                {
                    _isPressed[action] = true;
                    _pressedTime[action] += gameTime.GetElapsedSeconds();
                }
                else
                {
                    _isPressed[action] = false;
                    _pressedTime[action] = 0;
                }
            }
        }
    }
}